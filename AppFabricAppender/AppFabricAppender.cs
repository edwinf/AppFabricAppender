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
		private PatternLayout _Category;
		private bool _ImmediateFlush;

		public PatternLayout Category
		{
			get
			{
				return _Category;
			}
			set
			{
				_Category = value;
			}
		}

		public bool ImmediateFlush
		{
			get
			{
				return _ImmediateFlush;
			}
			set
			{
				_ImmediateFlush = value;
			}
		}

		public AppFabricAppender()
		{
			this._ImmediateFlush = true;
			this._Category = new PatternLayout("%logger");
		}

		protected override void Append(log4net.Core.LoggingEvent loggingEvent)
		{
			string val = base.RenderLoggingEvent(loggingEvent);
			
		}

	}
}
