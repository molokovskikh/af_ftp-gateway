using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using app.Models;
using Common.Models;
using Common.Models.Repositories;
using Common.MySql;
using Common.Tools;
using Common.Tools.Helpers;
using log4net;
using MySql.Data.MySqlClient;
using NHibernate;
using NHibernate.Linq;

namespace app.Protocols
{
	public static class Xml
	{
		private static ILog log = LogManager.GetLogger(typeof (Xml));

		public static void Price(XmlWriter writer, ActivePrice activePrice,
			IEnumerable<NamedOffer> offers)
		{
			var supplier = activePrice.Id.Price.Supplier;
			writer.WriteStartElement("PACKET");
			writer.WriteAttributeString("NAME", "Прайс-лист");
			writer.WriteAttributeString("FROM", supplier.Name);
			writer.WriteAttributeString("TYPE", "10");

			writer.WriteStartElement("PRICELIST");
			writer.WriteAttributeString("DATE", activePrice.PriceDate.ToString("dd.MM.yyyy HH:mm"));
			writer.WriteAttributeString("NAME", $"{supplier.Name} {activePrice.Id.Price.PriceName}");

			foreach (var offer in offers)
				ExportOffer(writer, offer);

			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		private static void ExportOffer(XmlWriter writer, NamedOffer offer)
		{
			writer.WriteStartElement("ITEM");

			writer.Element("CODE", String.Concat(offer.Code, offer.CodeCr));
			writer.Element("ACODE", offer.ProductId);
			writer.Element("ACODECR", offer.CodeFirmCr);
			writer.Element("NAME", offer.ProductSynonym);
			writer.Element("VENDOR", offer.ProducerSynonym);
			writer.Element("VENDORBARCODE", offer.EAN13.Slice(12));
			writer.Element("QTTY", offer.Quantity);
			writer.Element("VALID_DATE", offer.NormalizedPeriod);
			writer.Element("ISBAD", offer.Junk ? 1 : 0);
			writer.Element("COMMENT", offer.Note);
			writer.Element("XCODE", offer.Id.CoreId);
			writer.Element("MINQTTY", offer.MinOrderCount);
			writer.Element("MINSUM", offer.OrderCost);
			writer.Element("PACKQTTY", offer.RequestRatio);

			writer.WriteStartElement("PRICES");
			writer.Element("Базовая", offer.Cost);
			writer.WriteEndElement();

			writer.WriteEndElement();
		}


		public static void Waybill(ISession session, XmlWriter writer, Document document)
		{
			Order order = null;
			if (document.OrderId != null)
				order = session.Get<Order>(document.OrderId.Value);

			var lines = document.Lines;
			if (!lines.Any())
				return;

			writer.WriteStartElement("PACKET");
			writer.WriteAttributeString("NAME", "Электронная накладная");
			writer.WriteAttributeString("ID", document.ProviderDocumentId);
			writer.WriteAttributeString("FROM", document.Supplier.Name);
			writer.WriteAttributeString("TYPE", "12");

			writer.WriteStartElement("SUPPLY");
			writer.Element("INVOICE_NUM", document.ProviderDocumentId);
			writer.Element("INVOICE_DATE", document.DocumentDate.Value.ToString("dd.MM.yyyy"));
			writer.Element("DEP_ID", null);
			if (order != null)
				writer.Element("ORDER_ID", order.ClientOrderId);

			writer.WriteStartElement("ITEMS");
			foreach (var line in lines) {
				writer.WriteStartElement("ITEM");
				writer.Element("CODE", line.Code);
				writer.Element("NAME", line.Product);
				writer.Element("VENDOR", line.Producer);
				writer.Element("QTTY", line.Quantity);
				writer.Element("SPRICE", line.SupplierCostWithoutNDS);
				writer.Element("VPRICE", line.ProducerCostWithoutNDS);
				writer.Element("NDS", line.Nds);
				writer.Element("SNDSSUM", line.NdsAmount);
				writer.Element("SERIA", line.SerialNumber);
				writer.Element("VALID_DATE", line.Period);
				writer.Element("GTD", line.BillOfEntryNumber);
				writer.Element("SERT_NUM", line.Certificates);
				writer.Element("VENDORBARCODE", line.EAN13.Slice(12));
				writer.Element("REG_PRICE", line.RegistryCost);
				writer.Element("ISGV", line.VitallyImportant);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteEndElement();

			writer.WriteEndElement();
		}


		public static void OrderImport(uint userId, string file)
		{
			using (var session = Program.Factory.OpenSession())
			using (var trx = session.BeginTransaction()) {
				uint id;
				var log = new StringWriter();
				var rejects = new List<Reject>();
				var order = OrderParse(session, userId, XDocument.Load(file), rejects, log, out id);
				if (order != null)
					session.Save(order);
				trx.Commit();
			}
		}

		private static Order OrderParse(ISession session, uint userId, XDocument doc, List<Reject> rejects,
			TextWriter logForClient, out uint id)
		{
			var user = session.Load<User>(userId);
			id = 0;
			Order order = null;
			var packet = doc.XPathSelectElement("/PACKET");
			if (packet == null)
				return null;

			var type = (string) packet.Attribute("TYPE");
			if (type == null)
				return null;

			var predId = (string) packet.Attribute("PRED_ID");
			var to = (string) packet.Attribute("TO");
			var from = (string) packet.Attribute("FROM");
			var depId = (string) doc.XPathSelectElement("/PACKET/ORDER/DEP_ID");
			var orderElement = doc.XPathSelectElement("/PACKET/ORDER");
			var clientOrderId = SafeConvert.ToUInt32((string) orderElement.XPathSelectElement("ORDER_ID"));
			id = clientOrderId;

			var reject = new Reject {
				To = to,
				From = @from,
				PredId = predId,
				DepId = depId,
				OrderId = clientOrderId
			};
			var items = orderElement.XPathSelectElements("ITEMS/ITEM");
			foreach (var item in items) {
				var qunatity = SafeConvert.ToUInt32((string) item.XPathSelectElement("QTTY"));
				var cost = Convert.ToDecimal((string) item.XPathSelectElement("PRICE"));
				var offerId = SafeConvert.ToUInt64((string) item.XPathSelectElement("XCODE"));
				var code = (string) item.XPathSelectElement("CODE");
				var name = (string) item.XPathSelectElement("NAME");

				reject.Items.Add(new RejectItem(clientOrderId, code, qunatity, name, cost, offerId));
			}
			rejects.Add(reject);

			var addressIds = Program.GetAddressId(session, reject.PredId, reject.DepId.Slice(-2, -1), Program.SupplierIdForCodeLookup, user);
			var address = session.Load<Address>(addressIds[0]);
			var rules = session.Load<OrderRules>(user.Client.Id);
			rules.Strict = false;
			rules.CheckAddressAccessibility = false;
			List<ActivePrice> activePrices;
			using (StorageProcedures.GetActivePrices((MySqlConnection) session.Connection, userId)) {
				activePrices = session.Query<ActivePrice>().ToList();
			}

			var comment = (string) orderElement.XPathSelectElement("COMMENT");
			var existOrder =
				session.Query<Order>().FirstOrDefault(o => o.UserId == userId && o.ClientOrderId == clientOrderId && !o.Deleted);
			if (existOrder != null)
				throw new UserFriendlyException(
					$"Дублирующий заказ {clientOrderId}, существующий заказ {existOrder.RowId}",
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
						order.ClientAddition = comment;
						order.ClientOrderId = clientOrderId;
					}

					order.AddOrderItem(offer, item.Quantity);
					ordered.Add(item);
				} catch (OrderException e) {
					var message = Utils.TryGetUserFriendlyMessage(e);
					log.Warn($"Не удалось заказать позицию {item.Name} в количестве {item.Quantity}", e);
					logForClient.WriteLine("Не удалось заказать позицию {0} по заявке {3} в количестве {1}: {2}", item.Name,
						item.Quantity, message, clientOrderId);
				}
			}

			foreach (var rejectItem in ordered) {
				reject.Items.Remove(rejectItem);
			}

			if (reject.Items.Count == 0)
				rejects.Remove(reject);

			if (order != null && order.OrderItems.Count == 0)
				return null;

			return order;
		}


		public static void SaveInFile(string file, Action<XmlWriter> action)
		{
			var tmp = Path.GetTempFileName();
			try {
				var settings = new XmlWriterSettings {Encoding = Encoding.GetEncoding(1251)};
				using (var writer = XmlWriter.Create(tmp, settings)) {
					writer.WriteStartDocument(true);
					action(writer);
				}
				File.Delete(file);
				WaitHelper.WaitOrFail(TimeSpan.FromSeconds(30), () => !File.Exists(file),
					$"Не удалось дождаться удаления файла {file}");
				File.Move(tmp, file);
			} finally {
				File.Delete(tmp);
			}
		}
	}
}