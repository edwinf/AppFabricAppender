using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net.Layout;

namespace AppFabricAppender
{
	public class AppFabricAppender : log4net.Appender.AppenderSkeleton
	{
		private readonly static Type declaringType = typeof(AppFabricAppender);

		/// <summary>
		/// The cache configuration to push the logs to
		/// </summary>
		public List<AppFabricAppenderHost> Hosts { get; set; }

		/// <summary>
		/// The named cache to push the logs to.
		/// </summary>
		public string CacheName { get; set; }

		/// <summary>
		/// The Region to push the logs to
		/// </summary>
		public string RegionName { get; set; }

		public AppFabricAppender()
		{
			this.CacheName = "default";
			this.RegionName = Environment.MachineName;
			this.Hosts = new List<AppFabricAppenderHost>();
		}

		public override void ActivateOptions()
		{
			base.ActivateOptions();
		}

		public void AddHost(AppFabricAppenderHost host)
		{
			this.Hosts.Add(host);
		}

		protected override void Append(log4net.Core.LoggingEvent loggingEvent)
		{
			string val = base.RenderLoggingEvent(loggingEvent);

		}

	}
}
