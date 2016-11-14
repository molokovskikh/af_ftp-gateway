using System;
using System.IO;
using System.Linq;
using System.Reflection;
using app;
using app.Models;
using Common.Models;
using Common.Tools;
using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem.DotNet;
using log4net.Config;
using NHibernate.Linq;
using NUnit.Framework;
using test.DataFixture;
using Test.Support;
using Test.Support.Documents;
using Test.Support.Suppliers;

namespace test
{
	[TestFixture]
	public class FtpFixture : IntegrationFixture2
	{
		private int port;
		private FtpServer ftpServer;
		private TestSupplier supplier;
		private TestClient client;
		private TestUser user;

		[SetUp]
		public void Setup()
		{
			session.CreateSQLQuery("update Customers.Users set UseFtpGateway = 0").ExecuteUpdate();

			supplier = TestSupplier.CreateNaked(session);
			supplier.CreateSampleCore(session);
			client = TestClient.CreateNaked(session);
			user = client.Users[0];
			user.UseFtpGateway = true;
			user.FtpFileType = 2;
			Program.SupplierIdForCodeLookup = supplier.Id;
		}

		[OneTimeSetUp]
		public void OnetimeSetup()
		{
			port = new Random().Next(10000, 20000);
			var membershipProvider = new AnonymousMembershipProvider();
			var fsProvider = new DotNetFileSystemProvider("ftp", false);
			ftpServer = new FtpServer(fsProvider, membershipProvider, "127.0.0.1", port, new AssemblyFtpCommandHandlerFactory(typeof (FtpServer).GetTypeInfo().Assembly));
			ftpServer.Start();
		}

		[OneTimeTearDown]
		public void OnetimeTeardown()
		{
			ftpServer?.Dispose();
		}

		[Test]
		public void Export_prices()
		{
			FileHelper.InitDir("ftp");

			var log = new TestDocumentLog(supplier, client);
			session.Save(log);
			var doc = new TestWaybill(log);
			var product = session.Query<TestProduct>().First(x => !x.Hidden);
			doc.AddLine(product);
			doc.ProviderDocumentId = "G1";
			session.Save(doc);
			var sendLog = new TestDocumentSendLog(user, log);
			session.Save(sendLog);

			session.Save(new FtpConfig(session.Load<User>(user.Id), session.Load<Supplier>(supplier.Id)) {
				PriceUrl = $"ftp://localhost:{port}/тестовый поставщик/common/PRICE.DBF",
				WaybillUrl = $"ftp://localhost:{port}/тестовый поставщик/in",
				OrderUrl = $"ftp://localhost:{port}/тестовый поставщик/out",
			});

			FlushAndCommit();

			var job = new FtpExportJob();
			job.Execute(null);
			var prices = Directory.GetFiles("ftp/тестовый поставщик/common").Implode(Path.GetFileName);
			Assert.AreEqual("PRICE.DBF", prices);
			var waybills = Directory.GetFiles("ftp/тестовый поставщик/in").Implode(Path.GetFileName);
			Assert.AreEqual($"{log.Id}.dbf", waybills);
		}

		[Test]
		public void Import_order()
		{
			var address = client.Addresses[0];
			var price = supplier.Prices[0];
			var intersection =
				session.Query<TestAddressIntersection>().First(a => a.Address == address && a.Intersection.Price == price);
			intersection.SupplierDeliveryId = "1";
			session.Save(intersection);

			session.Save(new FtpConfig(session.Load<User>(user.Id), session.Load<Supplier>(supplier.Id)) {
				PriceUrl = $"ftp://localhost:{port}/тестовый поставщик/common/PRICE.DBF",
				WaybillUrl = $"ftp://localhost:{port}/тестовый поставщик/in",
				OrderUrl = $"ftp://localhost:{port}/тестовый поставщик/out",
			});

			FlushAndCommit();
			Directory.CreateDirectory("ftp/тестовый поставщик/out/");
			AsnaFixture.FillOrder("ftp/тестовый поставщик/out/order.dbf", new [] { price.Core[0].Id });

			var job = new FtpImportJob();
			job.Execute(null);
			var prices = Directory.GetFiles("ftp/тестовый поставщик/out").Implode(Path.GetFileName);
			Assert.AreEqual("", prices);

			var orders = session.Query<TestOrder>().Where(x => x.User == user).ToList();
			Assert.AreEqual(1, orders.Count);
		}
	}
}