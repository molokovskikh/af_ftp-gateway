using CassiniDev;
using NUnit.Framework;

namespace test
{
	[SetUpFixture]
	public class Setup
	{
		private Server _webServer;

		[OneTimeSetUp]
		public void SetupFixture()
		{
			string pathsToCleanBefore = "Customers.webftpoutsiders,billing.payerclients,Customers.Clients";
			SessionForQuery(pathsToCleanBefore);

			SeleniumFixture.GlobalSetup();
			_webServer = SeleniumFixture.StartServer();
		}

		[OneTimeTearDown]
		public void TeardownFixture()
		{
			string pathsToCleanAfter = "Customers.webftpoutsiders,billing.payerclients,Customers.Clients";
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