using System;
using app;
using Castle.ActiveRecord;
using NUnit.Framework;
using Test.Support;

namespace test
{
	[SetUpFixture]
	public class FixtureSetup
	{
		[OneTimeSetUp]
		public void Setup()
		{
			Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
			Test.Support.Setup.BuildConfiguration("db");
			var server = new app.Config.Initializers.NHibernate();
			var holder = ActiveRecordMediator.GetSessionFactoryHolder();
			server.Configuration = holder.GetConfiguration(typeof(ActiveRecordBase));
			server.Init();
			var sessionFactory = holder.GetSessionFactory(typeof(ActiveRecordBase));
			Program.Factory = sessionFactory;
			IntegrationFixture2.Factory = sessionFactory;
		}
	}
}