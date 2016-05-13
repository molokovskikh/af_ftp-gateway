using System;
using System.IO;
using System.Linq;
using app;
using Common.Tools;
using Common.Tools.Helpers;
using Ionic.Zip;
using NHibernate.Linq;
using NHibernate.Util;
using NUnit.Framework;
using Test.Support;
using Test.Support.log4net;
using Test.Support.Suppliers;

namespace test
{
	[TestFixture]
	public class DataFixture : IntegrationFixture2
	{
		private Program.Config config;

		[SetUp]
		public void Setup()
		{
			FileHelper.InitDir("tmp");
			config = new Program.Config();
			config.RootDir = Directory.CreateDirectory("tmp").FullName;
		}

		[Test]
		public void Export_prices()
		{
			var supplier = TestSupplier.CreateNaked(session);
			supplier.CreateSampleCore(session);
			var price = supplier.Prices[0];
			var client = TestClient.CreateNaked(session);
			FlushAndCommit();

			Directory.CreateDirectory("tmp/prices");
			FileHelper.Touch("tmp/prices/request.txt");
			Program.ProcessUser(config, client.Users[0].Id);
			Assert.That(Directory.GetFiles("tmp\\prices").Implode(), Does.Contain($"{price.Id}_1.xml"));
			Assert.IsFalse(File.Exists("tmp/prices/request.txt"));
		}

		[Test]
		public void Export_waybills()
		{
			var supplier = TestSupplier.CreateNaked(session);
			var client = TestClient.CreateNaked(session);
			var log = new TestDocumentLog(supplier, client);
			var doc = new TestWaybill(log);
			doc.AddLine(session.Query<TestProduct>().First(x => !x.Hidden));
			session.Save(log);
			Program.ProcessUser(config, client.Users[0].Id);
			Assert.IsFalse(File.Exists($"tmp/waybills/{log.Id}.xml"));
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

			Directory.CreateDirectory("tmp/orders");
			using (var zip = new ZipFile()) {
				zip.AddEntry("1.xml", OrderPacket(price.Core.Select(x => (object)x.Id).Take(2).ToArray()));
				zip.Save("tmp/orders/order.zip");
			}

			QueryCatcher.Warn();
			Program.ProcessUser(config, client.Users[0].Id);
			var orders = session.Query<TestOrder>().Where(x => x.Client.Id == client.Id).ToList();
			Assert.AreEqual(1, orders.Count);
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