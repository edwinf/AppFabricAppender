using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net.Util;
using Microsoft.ApplicationServer.Caching;

namespace log4netAppenders
{
	public class BufferedAppFabricAppender : log4net.Appender.BufferingAppenderSkeleton
	{
		private readonly static Type declaringType = typeof(BufferedAppFabricAppender);
		private DataCache _Cache;
		private string _CacheName;
		private ManualResetEventSlim _EmptyThreadQueueEvent;
		private int _ThreadQueueCount;
		private object lockObject = new object();

		internal DataCache Cache
		{
			get
			{
				return _Cache;
			}
		}

		internal long LastPushedKey { get; set; }

		/// <summary>
		/// The cache configuration to push the logs to
		/// </summary>
		public List<AppFabricAppenderHost> Hosts { get; set; }

		/// <summary>
		/// The named cache to push the logs to.
		/// </summary>
		public string CacheName
		{
			get
			{
				return this._CacheName ?? "default";
			}
			set
			{
				this._CacheName = value;
			}
		}

		/// <summary>
		/// The Region to push the logs to
		/// </summary>
		public string RegionName { get; set; }

		public BufferedAppFabricAppender()
		{
			this.CacheName = "default";
			this.RegionName = Shared.GetMachineName();
			this.Hosts = new List<AppFabricAppenderHost>();
			this._ThreadQueueCount = 0;
			this._EmptyThreadQueueEvent = new ManualResetEventSlim();
		}

		public override void ActivateOptions()
		{
			base.ActivateOptions();

			try
			{
				DataCacheFactory factory;
				if (this.Hosts.Count != 0)
				{
					LogLog.Debug(declaringType, "Activating host options");
					DataCacheFactoryConfiguration config = new DataCacheFactoryConfiguration();
					List<DataCacheServerEndpoint> servers = new List<DataCacheServerEndpoint>();
					for (int i = 0; i < this.Hosts.Count; i++)
					{
						servers.Add(new DataCacheServerEndpoint(this.Hosts[i].Host, this.Hosts[i].Port));
					}
					config.Servers = servers;
					factory = new DataCacheFactory(config);
				}
				else
				{
					LogLog.Debug(declaringType, "No host options detected, using default cache factory");
					factory = new DataCacheFactory();
				}
				_Cache = factory.GetCache(this.CacheName);

				//config region exists before we attempt to write to it.
				_Cache.CreateRegion(this.RegionName);

				var obj = _Cache.Get(Shared.LAST_PUSHED_KEY_KEY, this.RegionName);
				if (obj == null)
				{
					_Cache.Put(Shared.LAST_PUSHED_KEY_KEY, "0", this.RegionName);
				}
			}
			catch (Exception ex)
			{
				this.ErrorHandler.Error("Could not create connection to App Fabric", ex);
				this._Cache = null;
			}
		}

		public void AddHost(AppFabricAppenderHost host)
		{
			this.Hosts.Add(host);
		}

		protected override void OnClose()
		{
			base.OnClose();
			bool succeed = this._EmptyThreadQueueEvent.Wait(TimeSpan.FromSeconds(25));
			if (!succeed)
			{
				this.ErrorHandler.Error("Buffered App Fabric appender did not fully flush log queue before termination.");
			}
		}

		protected override void SendBuffer(log4net.Core.LoggingEvent[] events)
		{
			if (events != null && events.Length > 0)
			{
				//increment first then set the event in case of threading shenanigans on the EndSend 
				//(don't want to set then context switch and another come in and clear the event before the count is incremented)
				Interlocked.Increment(ref _ThreadQueueCount);
				this._EmptyThreadQueueEvent.Reset();

				//There is no bulk put unfortunatly, so spin off a thread and push them in one at a time.
				bool success = ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessEventsAsync), events);
				if (!success)
				{
					this.EndSend();
				}
			}
		}

		private void ProcessEventsAsync(object arg)
		{
			try
			{
				//Block any additional threads so that we aren't intermixing buffers since we can't write all the events at once
				lock (lockObject)
				{
					log4net.Core.LoggingEvent[] events = arg as log4net.Core.LoggingEvent[];
					if (events != null)
					{

						var obj = _Cache.Get(Shared.LAST_PUSHED_KEY_KEY,this.RegionName);
						if (obj != null)
						{
							this.LastPushedKey = Convert.ToInt64(obj);
						}
						else
						{
							this.LastPushedKey = 0;
						}
						

						for (int i = 0; i < events.Length; i++)
						{
							string val = base.RenderLoggingEvent(events[i]);
							this.LastPushedKey++;
							_Cache.Put(this.LastPushedKey.ToString(), val, this.RegionName);
						}
						_Cache.Put(Shared.LAST_PUSHED_KEY_KEY, this.LastPushedKey, this.RegionName);
					}
				}
			}
			finally
			{
				this.EndSend();
			}
		}

		private void EndSend()
		{
			if (Interlocked.Decrement(ref _ThreadQueueCount) <= 0)
			{
				this._EmptyThreadQueueEvent.Set();
			}
		}
	}
}

/*
 *  Copyright © 2012 edwinf (https://github.com/edwinf)
 *  
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
*/