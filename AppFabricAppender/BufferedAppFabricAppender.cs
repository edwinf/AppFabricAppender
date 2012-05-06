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
			this.RegionName = Environment.MachineName;
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
				//There is no bulk put unfortunatly, so spin off a thread and push them in one at a time.

				this._EmptyThreadQueueEvent.Reset();
				//atomic increment of the thread queue (in case of context switching)
				Interlocked.Increment(ref _ThreadQueueCount);
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
				//Block any additional threads so that we aren't intermixing buffers.
				lock(lockObject)
				{
					log4net.Core.LoggingEvent[] events = arg as log4net.Core.LoggingEvent[];
					for (int i = 0; i < events.Length; i++)
					{
						string val = base.RenderLoggingEvent(events[i]);
						_Cache.Put(Guid.NewGuid().ToString(), val, this.RegionName);
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
