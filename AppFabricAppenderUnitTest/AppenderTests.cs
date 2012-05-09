using System;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using log4netAppenders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppFabricAppenderUnitTest
{
	[TestClass]
	public class AppenderTests
	{

		private AppFabricAppender CreateDefaultAppender()
		{
			AppFabricAppender afap = new AppFabricAppender();
			afap.AddHost(new AppFabricAppenderHost() { Host = "127.0.0.1", Port=22233});
			afap.Layout = new SimpleLayout();
			
			afap.ActivateOptions();
			return afap;
		}

		private BufferedAppFabricAppender CreateBufferedAppender()
		{
			BufferedAppFabricAppender afap = new BufferedAppFabricAppender();
			afap.AddHost(new AppFabricAppenderHost() { Host = "127.0.0.1", Port = 22233 });
			afap.Layout = new SimpleLayout();
			afap.BufferSize = -1;
			afap.ActivateOptions();
			return afap;
		}

		[TestMethod]
		public void TestSyncronousPushAgainstDefaultCache()
		{
			ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());

			AppFabricAppender afap = CreateDefaultAppender();

			BasicConfigurator.Configure(rep, afap);

			ILog log = LogManager.GetLogger(rep.Name, "TestSyncronousPush");
			string origMessage = "This is a debug output";
			log.Debug(origMessage);

			var message = afap.Cache.Get(afap.LastPushedKey.ToString(), afap.RegionName);

			Assert.IsNotNull(message);
			Assert.IsTrue(string.Compare(message.ToString().Trim(), "DEBUG - "+origMessage) == 0);
		}

		[TestMethod]
		public void TestBufferedAppender()
		{
			ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());
			BufferedAppFabricAppender afap = CreateBufferedAppender();

			BasicConfigurator.Configure(rep, afap);

			ILog log = LogManager.GetLogger(rep.Name, "TestBufferedPush");
			string origMessage = "This is a debug output";
			log.Debug(origMessage);
			//wait for the async to process
			Thread.Sleep(5000);
			var message = afap.Cache.Get(afap.LastPushedKey.ToString(), afap.RegionName);

			Assert.IsNotNull(message);
			Assert.IsTrue(string.Compare(message.ToString().Trim(), "DEBUG - " + origMessage) == 0);
			
		}

		[TestMethod]
		public void SpeedTest()
		{
			//Trace.Listeners.Clear();
			//Trace.Listeners.Add(new DefaultTraceListener());

			//ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());

			//TraceAppender traceAppender = new TraceAppender();
			//traceAppender.Layout = new SimpleLayout();
			//traceAppender.ActivateOptions();
			//BasicConfigurator.Configure(rep, traceAppender);

			//ILog log = LogManager.GetLogger(rep.Name, GetType());
			//DateTime start = DateTime.Now;
			//for (int i = 0; i < 100000; i++)
			//{
			//	log.Debug(i.ToString());
			//}
			//DateTime end = DateTime.Now;

			//double timeForTrace = end.Subtract(start).TotalSeconds;

			//Debug.WriteLine("Total seconds for debug output to trace listener: " + timeForTrace);

			ILoggerRepository appfabricRep = LogManager.CreateRepository(Guid.NewGuid().ToString());

			AppFabricAppender afap = CreateDefaultAppender();

			BasicConfigurator.Configure(appfabricRep, afap);

			ILog appfabricLog = LogManager.GetLogger(appfabricRep.Name, "TestSyncronousPush");

			DateTime start = DateTime.Now;
			for (int i = 0; i < 100000; i++)
			{
				appfabricLog.Debug(i.ToString());
			}
			DateTime end = DateTime.Now;
			double timeForAppFabric = end.Subtract(start).TotalSeconds;

			Debug.WriteLine("Total seconds for debug output to app fabric: " + timeForAppFabric);
		}

		[TestMethod]
		public void TestXMLConfig()
		{
			string xml = @"<log4net>
									<appender name=""AppFabricAppender"" type=""log4netAppenders.AppFabricAppender, AppFabricAppender-log4net"">
										<host>
											<host>127.0.0.1</host>
											<port>22233</port>
										</host>
										<layout type=""log4net.Layout.PatternLayout"" value=""%date [%thread] %-5level %logger - %message%newline"" />
									</appender>
									<root>
										<level value=""ALL"" />
										<appender-ref ref=""AppFabricAppender"" />
									</root>
								</log4net>";
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());
			XmlConfigurator.Configure(rep, doc["log4net"]);

			ILog log = LogManager.GetLogger(rep.Name, "XMLText");

			log.Debug("Output");
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