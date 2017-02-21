using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using app;
using app.Models;
using Common.Models;
using Common.Models.Repositories;
using Common.MySql;
using Common.Tools;
using log4net;
using MySql.Data.MySqlClient;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Util;
using System.Linq;

namespace app.Protocols
{
	//Регард софт
	public static class Dbf
	{
		private static ILog log = LogManager.GetLogger(typeof (Dbf));

		public static DataTable Price(ActivePrice activePrice, IEnumerable<NamedOffer> offers)
		{
			var table = new DbfTable();
			table.Columns(
				Column.Char("CODEPST", 12),
				Column.Char("NAME", 80),
				Column.Char("CNTR", 15),
				Column.Char("FIRM", 40),
				Column.Numeric("QNTPACK", 8),
				Column.Char("EAN13", 13),
				Column.Numeric("NDS", 9, 2),
				Column.Date("GDATE"),
				Column.Numeric("QNT", 9, 2),
				Column.Numeric("NSP", 9, 2),
				Column.Numeric("GNVLS", 1),
				Column.Numeric("PRICE1", 9, 2),
				Column.Numeric("NEWFLG", 1),
				Column.Numeric("PROTECID", 8),
				Column.Numeric("QNTPST", 8),
				Column.Numeric("XCODE", 20));

			foreach (var offer in offers) {
				table.Row(
					Value.For("CODEPST", offer.Code),
					Value.For("NAME", offer.ProductSynonym),
					//Value.For("CNTR", offer.),
					Value.For("FIRM", offer.ProducerSynonym),
					Value.For("QNTPACK", offer.RequestRatio),
					Value.For("EAN13", offer.EAN13.Slice(12)),
					//Value.For("NDS", offer.),
					Value.For("GDATE", offer.NormalizedPeriod), // !
					Value.For("QNT", offer.Quantity),
					//Value.For("NSP", offer.),
					Value.For("GNVLS", offer.VitallyImportant),
					Value.For("PRICE1", offer.Cost),
					//Value.For("NEWFLG", offer.)
					//Value.For("PROTECID", offer.)
					//Value.For("QNTPST", offer.)
					Value.For("XCODE", offer.CoreId)
					);
			}
			return table.ToDataTable();
		}

		public static DataTable Waybll(ISession session, Document document)
		{
			var lines = document.Lines;
			if (!lines.Any())
				return null;

			var table = new DbfTable();
			table.Columns(
				Column.Char("NDOC", 20),
				Column.Date("DATEDOC"),
				Column.Char("CODEPST", 12),
				Column.Char("EAN13", 13),
				Column.Numeric("PRICE1", 9, 2),
				Column.Numeric("PRICE2", 9, 2),
				Column.Numeric("PRICE2N", 9, 2),
				Column.Numeric("PRCIMP", 9, 2),
				Column.Numeric("PRCOPT", 9, 2),
				Column.Numeric("QNT", 9, 2),
				Column.Char("SER", 20),
				Column.Date("GDATE"),
				Column.Date("DATEMADE"),
				Column.Char("NAME", 80),
				Column.Char("CNTR", 15),
				Column.Char("FIRM", 40),
				Column.Numeric("QNTPACK", 8),
				Column.Numeric("NDS", 9, 2),
				Column.Numeric("NSP", 9, 2),
				Column.Numeric("GNVLS", 1),
				Column.Numeric("REGPRC", 9, 2),
				Column.Date("DATEPRC"),
				Column.Char("NUMGTD", 30),
				Column.Char("SERTIF", 80),
				Column.Date("SERTDATE"),
				Column.Char("SERTORG", 80),
				Column.Numeric("SUMPAY", 9, 2),
				Column.Numeric("SUMNDS10", 9, 2),
				Column.Numeric("SUMNDS20", 9, 2),
				Column.Numeric("SUM10", 9, 2),
				Column.Numeric("SUM20", 9, 2),
				Column.Numeric("SUM0", 9, 2),
				Column.Numeric("EXCHCODE", 1),
				Column.Numeric("ERATE", 9, 4),
				Column.Char("PODRCD", 12),
				Column.Numeric("NUMZ", 8),
				Column.Date("DATEZ"),
				Column.Char("BILLNUM", 20),
				Column.Date("BILLDT"),
				Column.Numeric("PAYID", 2),
				Column.Numeric("SELLERID", 6),
				Column.Numeric("PRCR", 9, 2),
				Column.Numeric("PRICER", 9, 2),
				Column.Numeric("EGKCODE", 6),
				Column.Char("CNTRMADE", 15),
				Column.Date("SERTGIVE"),
				Column.Numeric("DSCFIN", 4, 2),
				Column.Numeric("DSCFINS", 4, 2),
				Column.Bool("DSCNTR"),
				Column.Bool("DSCNTRS"),
				Column.Numeric("SUMS0", 12, 2),
				Column.Numeric("SUMSNDS", 12, 2));

			foreach (var line in lines) {
				table.Row(
					Value.For("NDOC", document.ProviderDocumentId),
					Value.For("DATEDOC", document.DocumentDate),
					Value.For("CODEPST", line.Code),
					Value.For("EAN13", line.EAN13.Slice(12)),
					Value.For("PRICE1", line.ProducerCostWithoutNDS),
					Value.For("PRICE2", line.SupplierCost),
					Value.For("PRICE2N", line.SupplierCostWithoutNDS),
					//Value.For("PRCIMP", line.),
					//Value.For("PRCOPT", line.),
					Value.For("QNT", line.Quantity),
					Value.For("SER", line.SerialNumber),
					Value.For("GDATE", line.Period), // !
					Value.For("DATEMADE", line.DateOfManufacture),
					Value.For("NAME", line.Product),
					Value.For("CNTR", line.Country),
					Value.For("FIRM", line.Producer),
					Value.For("QNTPACK", line.Code),
					Value.For("NDS", line.Nds),
					//Value.For("NSP", line.),
					Value.For("GNVLS", line.VitallyImportant),
					Value.For("REGPRC", line.RegistryCost),
					Value.For("DATEPRC", line.RegistryDate),
					Value.For("NUMGTD", line.BillOfEntryNumber),
					Value.For("SERTIF", line.Certificates),
					Value.For("SERTDATE", line.CertificatesDate), // !
					Value.For("SERTORG", line.CertificateAuthority),
					Value.For("SUMPAY", document.Invoice?.Amount),
					Value.For("SUMNDS10", document.Invoice?.NDSAmount10),
					Value.For("SUMNDS20", document.Invoice?.NDSAmount18),
					Value.For("SUM10", document.Invoice?.AmountWithoutNDS10),
					Value.For("SUM20", document.Invoice?.AmountWithoutNDS18),
					//Value.For("SUM0", line.),
					//Value.For("EXCHCODE", line.),
					//Value.For("ERATE", line.),
					//Value.For("PODRCD", line.),
					//Value.For("NUMZ", document.ProviderDocumentId),
					//Value.For("DATEZ", document.DocumentDate),
					Value.For("BILLNUM", document.ProviderDocumentId),
					Value.For("BILLDT", document.DocumentDate),
					//Value.For("PAYID", line.),
					//Value.For("SELLERID", line.),
					//Value.For("PRCR", line.),
					//Value.For("PRICER", line.),
					//Value.For("EGKCODE", line.),
					//Value.For("CNTRMADE", line.),
					//Value.For("SERTGIVE", line.)
					//Value.For("DSCFIN", line.),
					//Value.For("DSCFINS", line.),
					//Value.For("DSCNTR", line.),
					//Value.For("DSCNTRS", line.),
					Value.For("SUMS0", line.Amount),
					Value.For("SUMSNDS", line.NdsAmount)
					);
			}
			return table.ToDataTable();
		}


		public static void OrderImport(uint userId, string file)
		{
			using (var session = Program.Factory.OpenSession())
			using (var trx = session.BeginTransaction()) {
				var table = Common.Tools.Dbf.Load(file);
				var order = OrderParse(session, userId, table);
				if (order != null)
					session.Save(order);
				trx.Commit();
			}
		}

		private static Order OrderParse(ISession session, uint userId, DataTable table)
		{
			var user = session.Load<User>(userId);
			Order order = null;
			if (table.Rows.Count == 0)
				return null;

			var supplierDeliveryId = table.Rows[0]["PODRCD"].ToString();
			uint id;
			unchecked {
				id = (uint)table.Rows[0]["NUMZ"].GetHashCode();
			}

			var reject = new Reject {
				DepId = supplierDeliveryId,
				OrderId = id
			};
			foreach (DataRow row in table.Rows) {
				uint qunatity;
				try {
					qunatity = Convert.ToUInt32(row["QNT"]);
				} catch(Exception) {
					qunatity = SafeConvert.ToUInt32(row["QNT"].ToString());
				}
				var cost = Convert.ToDecimal(row["PRICE"]);
				var offerId = SafeConvert.ToUInt64(row["XCODE"].ToString());
				var code = row["CODEPST"].ToString();
				var name = row["NAME"].ToString();

				reject.Items.Add(new RejectItem(id, code, qunatity, name, cost, offerId));
			}

			var addressIds = GetAddressId(session, supplierDeliveryId, null, Program.SupplierIdForCodeLookup, user);

			var address = session.Load<Address>(addressIds[0]);
			var rules = session.Load<OrderRules>(user.Client.Id);
			rules.Strict = false;
			rules.CheckAddressAccessibility = false;
			List<ActivePrice> activePrices;
			using (StorageProcedures.GetActivePrices((MySqlConnection) session.Connection, userId)) {
				activePrices = session.Query<ActivePrice>().ToList();
			}

			var existOrder =
				session.Query<Order>().FirstOrDefault(o => o.UserId == userId && o.ClientOrderId == id && !o.Deleted);
			if (existOrder != null)
				throw new UserFriendlyException(
					$"Дублирующий заказ {id}, существующий заказ {existOrder.RowId}",
					"Дублирующая заявка");

			var ordered = new List<RejectItem>();
			foreach (var item in reject.Items) {
				try {
					var offer = OfferQuery.GetById(session, user, item.OfferId);

					if (offer == null) {
						var archiveOffer = session.Get<ArchiveOffer>(item.OfferId);
						if (archiveOffer == null)
							throw new UserFriendlyException($"Не удалось найти предложение {item.OfferId} игнорирую строку",
								"Заявка сделана по неактуальному прайс-листу");

						offer = archiveOffer.ToOffer(activePrices, item.Price);
						if (offer == null)
							throw new UserFriendlyException(
								$"Прайс {archiveOffer.PriceList.PriceCode} больше не доступен клиенту игнорирую строку",
								"Прайс-лист отключен");
					}

					if (order == null) {
						order = new Order(offer.PriceList, user, address, rules);
						order.ClientOrderId = id;
					}

					order.AddOrderItem(offer, item.Quantity);
					ordered.Add(item);
				} catch (OrderException e) {
					log.Warn($"Не удалось заказать позицию {item.Name} в количестве {item.Quantity}", e);
				}
			}

			foreach (var rejectItem in ordered) {
				reject.Items.Remove(rejectItem);
			}

			if (order != null && order.OrderItems.Count == 0)
				return null;

			return order;
		}


		private static IList<uint> GetAddressId(ISession session, string supplierDeliveryId, string supplierClientId,
			uint supplierId, User user)
		{
			//пустая строка должна интерпретироваться как null
			supplierClientId = supplierClientId?.Trim();
			supplierDeliveryId = supplierDeliveryId?.Trim();
			if (String.IsNullOrEmpty(supplierClientId))
				supplierClientId = null;
			if (String.IsNullOrEmpty(supplierDeliveryId))
				supplierDeliveryId = null;
			var addressIds = session.CreateSQLQuery(@"
select ai.AddressId
from Customers.Intersection i
	join Customers.AddressIntersection ai on ai.IntersectionId = i.Id
	join Usersettings.Pricesdata pd on pd.PriceCode = i.PriceId
		join Customers.Suppliers s on s.Id = pd.FirmCode
where i.SupplierClientId <=> :supplierClientId
	and ai.SupplierDeliveryId <=> :supplierDeliveryId
	and i.ClientId = :clientId
	and s.Id = :supplierId
group by ai.AddressId")
				.SetParameter("supplierClientId", supplierClientId)
				.SetParameter("supplierDeliveryId", supplierDeliveryId)
				.SetParameter("supplierId", supplierId)
				.SetParameter("clientId", user.Client.Id)
				.List<uint>();

			if (addressIds.Count == 0) {
				throw new UserFriendlyException(
					$"Не удалось найти адрес доставки supplierClientId = {supplierClientId} supplierDeliveryId = {supplierDeliveryId} игнорирую заказ",
					"Неизвестный отправитель");
			}
			return addressIds;
		}
	}
}