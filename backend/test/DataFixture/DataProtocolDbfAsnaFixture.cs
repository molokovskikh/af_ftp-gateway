using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using app;
using app.Config;
using app.Protocols;
using Common.Tools;
using Ionic.Zip;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support;
using Test.Support.Documents;
using Test.Support.Suppliers;

namespace test.DataFixture
{
	[TestFixture]
	public class DataProtocolDbfAsnaFixture : IntegrationFixture2
	{
		private Config config;

		[SetUp]
		public void Setup()
		{
			FileHelper.InitDir("tmp");
			config = new Config();
			config.RootDir = Directory.CreateDirectory("tmp").FullName;
		}


		[Test]
		public void Export_waybills()
		{
			var supplier = TestSupplier.CreateNaked(session);
			var client = TestClient.CreateNaked(session);
			var log = new TestDocumentLog(supplier, client);
			session.Save(log);
			var doc = new TestWaybill(log);
			var product = session.Query<TestProduct>().First(x => !x.Hidden);
			doc.AddLine(product);
			doc.ProviderDocumentId = "G1";
			session.Save(doc);
			var sendLog = new TestDocumentSendLog(client.Users[0], log);
			session.Save(sendLog);
			FlushAndCommit();
			Program.ProcessUser(config, client.Users[0].Id, ProtocolType.DbfAsna);
			Assert.IsTrue(File.Exists($"tmp/{client.Users[0].Id}/waybills/{doc.Id}.dbf"));
		}

		[Test]
		public void Export_prices_dbf()
		{
			var supplier = TestSupplier.CreateNaked(session);
			supplier.CreateSampleCore(session);
			var price = supplier.Prices[0];
			var client = TestClient.CreateNaked(session);
			FlushAndCommit();

			var root = Directory.CreateDirectory($"tmp/{client.Users[0].Id}/prices");
			FileHelper.Touch(Path.Combine(root.FullName, "request.txt"));
			Program.ProcessUser(config, client.Users[0].Id, ProtocolType.DbfAsna);
			Assert.That(root.GetFiles().Implode(), Does.Contain($"{price.Id}_1.dbf"));
			Assert.IsFalse(File.Exists(Path.Combine(root.FullName, "request.txt")));
		}


		[Test]
		public void Import_dbf_order()
		{
			var supplier = TestSupplier.CreateNaked(session);
			supplier.CreateSampleCore(session);
			Program.SupplierIdForCodeLookup = supplier.Id;
			var price = supplier.Prices[0];
			var client = TestClient.CreateNaked(session);
			var address = client.Addresses[0];
			var intersection =
				session.Query<TestAddressIntersection>().First(a => a.Address == address && a.Intersection.Price == price);
			intersection.SupplierDeliveryId = "1";
			session.Save(intersection);
			FlushAndCommit();

			var root = Directory.CreateDirectory($"tmp/{client.Users[0].Id}/orders/");
			var table = FillOrder(price.Core.Select(x => (object) x.Id).Take(2).ToArray());

			using (var file = new StreamWriter(File.Create(Path.Combine(root.FullName, "order.dbf")), Encoding.GetEncoding(866)))
				Dbf2.SaveAsDbf4(table, file);

			Program.ProcessUser(config, client.Users[0].Id, ProtocolType.DbfAsna);
			var orders = session.Query<TestOrder>().Where(x => x.Client.Id == client.Id).ToList();
			Assert.AreEqual(1, orders.Count);
		}

		protected DataTable FillOrder(object[] ids)
		{
			var table = new DbfTable();
			table.Columns(
				Column.Numeric("NUMZ", 8),
				Column.Date("DATEZ"),
				Column.Char("CODEPST", 12),
				Column.Numeric("PAYID", 2),
				Column.Date("DATE"),
				Column.Char("PODR", 40),
				Column.Numeric("QNT", 8),
				Column.Numeric("PRICE", 9, 2),
				Column.Char("PODRCD", 12),
				Column.Char("NAME", 80),
				Column.Numeric("XCODE", 20)); // расширение протокола

			table.Row(
				Value.For("NUMZ", 2001),
				Value.For("DATEZ", DateTime.Now),
				Value.For("CODEPST", "135"),
				Value.For("PAYID", 1), // по колонке PRICE1 прайслиста
				Value.For("DATE", DateTime.Now),
				Value.For("PODR", "аптека"),
				Value.For("QNT", 1),
				Value.For("PRICE", 39.94),
				Value.For("PODRCD", "1"),
				Value.For("NAME", "АНАЛЬГИН АМП. 50% 2МЛ N10 РОССИЯ"),
				Value.For("XCODE", ids[0])
				);

			return table.ToDataTable();
		}
	}
}