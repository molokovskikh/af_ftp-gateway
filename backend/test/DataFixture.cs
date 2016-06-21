using System;
using System.IO;
using System.Linq;
using app;
using app.Config;
using Common.Tools;
using Common.Tools.Helpers;
using Ionic.Zip;
using NHibernate.Linq;
using NHibernate.Util;
using NUnit.Framework;
using Test.Support;
using Test.Support.log4net;
using Test.Support.Suppliers;
using Test.Support.Documents;
using System.Data;
using app.Dbf;
using System.Threading;

namespace test
{
	[TestFixture]
	public class DataFixture : IntegrationFixture2
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
		public void Export_prices_xml()
		{
			var supplier = TestSupplier.CreateNaked(session);
			supplier.CreateSampleCore(session);
			var price = supplier.Prices[0];
			var client = TestClient.CreateNaked(session);
			FlushAndCommit();

			var root = Directory.CreateDirectory($"tmp/{client.Users[0].Id}/prices");
			FileHelper.Touch(Path.Combine(root.FullName, "request.txt"));
			Program.ProcessUser(config, client.Users[0].Id, 0);
			Assert.That(root.GetFiles().Implode(), Does.Contain($"{price.Id}_1.xml"));
			Assert.IsFalse(File.Exists(Path.Combine(root.FullName, "request.txt")));
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
			Program.ProcessUser(config, client.Users[0].Id, 1);
			Assert.That(root.GetFiles().Implode(), Does.Contain($"{price.Id}_1.dbf"));
			Assert.IsFalse(File.Exists(Path.Combine(root.FullName, "request.txt")));
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
			Program.ProcessUser(config, client.Users[0].Id, 0);
			Assert.IsTrue(File.Exists($"tmp/{client.Users[0].Id}/waybills/{doc.Id}.xml"));
			Assert.IsTrue(File.Exists($"tmp/{client.Users[0].Id}/waybills/{doc.Id}.dbf"));
		}

		[Test]
		public void Import_order()
		{
			var supplier = TestSupplier.CreateNaked(session);
			supplier.CreateSampleCore(session);
			Program.SupplierIdForCodeLookup = supplier.Id;
			var price = supplier.Prices[0];
			var client = TestClient.CreateNaked(session);
			var address = client.Addresses[0];
			var intersection = session.Query<TestAddressIntersection>().First(a => a.Address == address && a.Intersection.Price == price);
			intersection.SupplierDeliveryId = "02";
			intersection.Intersection.SupplierClientId = "1";
			session.Save(intersection);
			FlushAndCommit();

			var root = Directory.CreateDirectory($"tmp/{client.Users[0].Id}/orders/");
			using (var zip = new ZipFile()) {
				zip.AddEntry("1.xml", OrderPacket(price.Core.Select(x => (object)x.Id).Take(2).ToArray()));
				zip.Save(Path.Combine(root.FullName, "order.zip"));
			}

			QueryCatcher.Warn();
			Program.ProcessUser(config, client.Users[0].Id, 0);
			var orders = session.Query<TestOrder>().Where(x => x.Client.Id == client.Id).ToList();
			Assert.AreEqual(1, orders.Count);
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
			var intersection = session.Query<TestAddressIntersection>().First(a => a.Address == address && a.Intersection.Price == price);
			intersection.SupplierDeliveryId = null;
			intersection.Intersection.SupplierClientId = "1";
			session.Save(intersection);
			FlushAndCommit();

			var root = Directory.CreateDirectory($"tmp/{client.Users[0].Id}/orders/");
			var table = FillOrder(price.Core.Select(x => (object)x.Id).Take(2).ToArray());
			Common.Tools.Dbf.Save(table, Path.Combine(root.FullName, "order.dbf"));

			QueryCatcher.Warn();
			Program.ProcessUser(config, client.Users[0].Id, 1);
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

		protected string OrderPacket(object[] ids)
		{
			var data = @"<?xml version=""1.0"" encoding=""windows-1251"" standalone=""yes"" ?>
<PACKET TYPE=""11"" NAME=""Заявка поставщику"" ID=""2000"" PRED_ID=""1"" FROM=""001"" TO=""АПТЕКА-ХОЛДИНГ"">
  <ORDER>
    <ORDER_ID>2000</ORDER_ID>
    <DEP_ID>1002</DEP_ID>
    <CLIENT_ID>1002</CLIENT_ID>
    <ORDERDATE>18.08.2010 13:00:30</ORDERDATE>
    <PLDATE>18.08.2010 12:33:05</PLDATE>
    <PAYTYPE>C143_D_отсрочка_3</PAYTYPE>
    <COMMENT>аптека</COMMENT>
    <ITEMS>
      <ITEM>
        <CODE>135</CODE>
        <NAME>АНАЛЬГИН АМП. 50% 2МЛ N10 РОССИЯ</NAME>
        <VENDOR>ДАЛЬХИМФАРМ ОАО</VENDOR>
        <QTTY>1</QTTY>
        <PRICE>39,94</PRICE>
        <XCODE>{0}</XCODE>
      </ITEM>
      <ITEM>
        <CODE>15952</CODE>
        <NAME>АНАЛЬГИН АМП. 50% 2МЛ N10 РОССИЯ</NAME>
        <VENDOR>ВИРИОН (МИКРОГЕН НПО ФГУП) Г. ТОМСК</VENDOR>
        <QTTY>1</QTTY>
        <PRICE>41,71</PRICE>
        <XCODE>{1}</XCODE>
      </ITEM>
    </ITEMS>
  </ORDER>
</PACKET>
";
			data = String.Format(data, ids);
			return data;
		}
	}
}
