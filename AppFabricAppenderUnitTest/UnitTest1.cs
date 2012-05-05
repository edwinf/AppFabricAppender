using System;
using log4net;
using log4net.Config;
using log4net.Layout;
using log4net.Repository;
using log4netAppenders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppFabricAppenderUnitTest
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestSyncronousPushAgainstDefaultCache()
		{
			ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());

			AppFabricAppender afap = new AppFabricAppender();
			afap.AddHost(new AppFabricAppenderHost() { Host = "127.0.0.1", Port=22233});
			afap.Layout = new SimpleLayout();

			afap.ActivateOptions();

			BasicConfigurator.Configure(rep, afap);

			ILog log = LogManager.GetLogger(rep.Name, "TestSyncronousPush");
			log.Debug("This is a debug output");

		}
	}
}
