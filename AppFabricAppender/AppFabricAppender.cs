using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net.Layout;
using log4net.Util;
using Microsoft.ApplicationServer.Caching;

namespace log4netAppenders
{
	public class AppFabricAppender : log4net.Appender.AppenderSkeleton
	{
		private readonly static Type declaringType = typeof(AppFabricAppender);
		private DataCache _Cache;
		private string _CacheName;

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

		public AppFabricAppender()
		{
			this.CacheName = "default";
			this.RegionName = Shared.GetMachineName();
			this.Hosts = new List<AppFabricAppenderHost>();
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

		

		protected override void Append(log4net.Core.LoggingEvent loggingEvent)
		{
			if (_Cache == null)
			{
				//we couldn't initialize the cache initially, don't mess up performance by trying to connect every time we log.
				return;
			}
			string val = base.RenderLoggingEvent(loggingEvent);
			string currentKey = (++this.LastPushedKey).ToString();
			_Cache.Put(currentKey, val, this.RegionName);
			_Cache.Put(Shared.LAST_PUSHED_KEY_KEY, currentKey, this.RegionName);
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
