using System;
using System.Drawing;
using CassiniDev;
using Castle.ActiveRecord;
using NUnit.Framework;
using Test.Support;
using Test.Support.Selenium;

namespace test
{
	[SetUpFixture]
	public class Setup
	{
		private Server _webServer;

		[OneTimeSetUp]
		public void SetupFixture()
		{
			Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
			SeleniumFixture.GlobalSetup();

			_webServer = SeleniumFixture.StartServer("../../../app/");
			SeleniumFixture.GlobalDriver.Manage().Window.Size = new Size(1920, 1080);
			Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
			Test.Support.Setup.BuildConfiguration("db");
			var holder = ActiveRecordMediator.GetSessionFactoryHolder();
			var cfg = holder.GetConfiguration(typeof(ActiveRecordBase));
			var init = new web_app.NHibernate();
			init.Configuration = cfg;
			init.Init();
			var factory = holder.GetSessionFactory(typeof(ActiveRecordBase));
			Test.Support.IntegrationFixture2.Factory = factory;
		}

		[OneTimeTearDown]
		public void TeardownFixture()
		{
			string pathsToCleanAfter = "Customers.ftpconfigs,Customers.webftpoutsiders,billing.payerclients,Customers.Clients";
			SessionForQuery(pathsToCleanAfter);
			SeleniumFixture.GlobalTearDown();
			_webServer.ShutDown();
		}

		private void SessionForQuery(string query)
		{
			var nhibernate = new web_app.NHibernate();
			nhibernate.Init();
			var factory = nhibernate.Factory;
			var dbSession = factory.OpenSession();
			foreach (var path in query.Split(',')) {
				dbSession.CreateSQLQuery($"DELETE FROM {path}").UniqueResult();
			}
			dbSession.Close();
		}
	}
}