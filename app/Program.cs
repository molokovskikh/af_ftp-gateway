using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using app.Models;
using Common.Models;
using Common.Models.Repositories;
using Common.MySql;
using Common.NHibernate;
using Common.Tools;
using Common.Tools.Threading;
using Ionic.Zip;
using log4net;
using log4net.Config;
using MySql.Data.MySqlClient;
using NHibernate;
using NHibernate.Linq;

namespace app
{
	public class UserFriendlyException : OrderException
	{
		public UserFriendlyException(string message, string usermessage) : base(message)
		{
			UserMessage = usermessage;
		}

		public string UserMessage { get; set; }
	}

	public static class XmlWriterExtentions
	{
		public static void Element(this XmlWriter writer, string name, object value)
		{
			value = NullableHelper.GetNullableValue(value);
			if (value == null
				|| value.Equals(String.Empty))
				return;

			if (value is bool) {
				value = (bool)value ? 1 : 0;
			}

			writer.WriteElementString(name, value.ToString());
		}
	}

	public class NamedOffer : Offer
	{
		public NamedOffer()
		{
			Id = new OfferKey();
		}

		public string ProductSynonym { get; set; }
		public string ProducerSynonym { get; set; }

		public string NormalizedPeriod
		{
			get
			{
				DateTime date;
				if (DateTime.TryParse(Period, out date))
					return date.ToShortDateString();
				return null;
			}
		}

		public ulong CoreId
		{
			get { return Id.CoreId; }
			set { Id.CoreId = value; }
		}

		public ulong RegionId
		{
			get { return Id.RegionCode; }
			set { Id.RegionCode = value; }
		}
	}

	public class Program
	{
		public static ISessionFactory Factory;
		public static uint SupplierIdForCodeLookup;

		private static ILog log = LogManager.GetLogger(typeof(Program));

		public static int Main(string[] args)
		{
			CancellationTokenSource stop = null;
			try
			{
				AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => {
					log.Fatal("Необработанная ошибка", eventArgs.ExceptionObject as Exception);
				};
				TaskScheduler.UnobservedTaskException += (sender, eventArgs) => {
					log.Fatal("Необработанная ошибка", eventArgs.Exception);
				};

				var config = new Config.Config(ConfigurationManager.AppSettings);
				XmlConfigurator.Configure();
				stop = new CancellationTokenSource();
				SupplierIdForCodeLookup = config.SupplierId;

				var nHibernate = new Config.Initializers.NHibernate();
				nHibernate.Init();
				Factory = nHibernate.Factory;

				return CommandService.Start(args, stop, new Task(() => MainLoop(config, stop.Token)));
			}
			catch (Exception e) {
				log.Fatal("Не удалось запустить приложение", e);
				//если протоколирование сломано
				Console.Error.WriteLine(e);
				stop?.Cancel();
				return 1;
			}
		}

		public static void MainLoop(Config.Config config, CancellationToken token)
		{
			try {
				IList<uint> userIds;
				using (var session = Factory.OpenSession()) {
					userIds = session.CreateSQLQuery("select Id from Customers.Users where UseFtpGateway = 1")
						.List<uint>();
				}
				foreach (var userId in userIds) {
					log.Debug($"Обработка пользователя {userId}");
					token.ThrowIfCancellationRequested();
					ProcessUser(config, userId);
				}
				token.WaitHandle.WaitOne(config.LookupTime);
				token.ThrowIfCancellationRequested();
			} catch(Exception e) {
				if (e is OperationCanceledException)
					return;
				log.Error("Ошибка при обработке", e);
			}
		}

		public static void ProcessUser(Config.Config config, uint userId)
		{
			var userRoot = Path.Combine(config.RootDir, userId.ToString());
			var pricesDir = Directory.CreateDirectory(Path.Combine(userRoot, "prices"));
			var marker = pricesDir.EnumerateFiles("request.txt").FirstOrDefault();
			if (marker != null) {
				using (var session = Factory.OpenSession())
				using (var trx = session.BeginTransaction()) {
					ExportPrices(session, userId, pricesDir);
					trx.Commit();
				}
				marker.Delete();
			}

			var ordersDir = Directory.CreateDirectory(Path.Combine(userRoot, "orders"));
			ImportOrders(ordersDir, userId);

			var waybillsDir = Directory.CreateDirectory(Path.Combine(userRoot, "waybills"));
			using (var session = Factory.OpenSession())
			using (var trx = session.BeginTransaction()) {
				ExportWaybills(waybillsDir, session, userId);
				trx.Commit();
			}
		}

		private static void ExportWaybills(DirectoryInfo root, ISession session, uint userId)
		{
			var sendedWaybills = session.CreateSQLQuery(@"
select dh.Id
from Logs.DocumentSendLogs ds
	join Logs.Document_logs d on d.RowId = ds.DocumentId
	join Documents.DocumentHeaders dh on dh.DownloadId = d.RowId
where ds.UserId = :userId
	and ds.Committed in (0, 2)
order by d.LogTime desc
limit 400;")
				.SetParameter("userId", userId)
				.List<uint>();
			foreach (var id in sendedWaybills) {
				var name = Path.Combine(root.FullName, id + ".xml");
				var doc = session.Load<Document>(id);

				Export(name, w => ExportWaybill(session, w, doc));
			}
		}

		public static void ExportWaybill(ISession session, XmlWriter writer, Document document)
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

		private static void ImportOrders(DirectoryInfo root, uint userId)
		{
			var files = Directory.GetFiles(root.FullName);
			foreach (var file in files) {
				try {
					var ext = Path.GetExtension(file);
					if (ext.Match(".xml")) {
						ImportOrder(userId, file);
						File.Delete(file);
					} else if (ext.Match(".zip") || ext.Match(".ord")) {
						using (var zip = ZipFile.Read(file)) {
							foreach (var zipItem in zip) {
								if (Path.GetExtension(zipItem.FileName).Match(".xml")) {
									var tmp = Path.GetTempFileName();
									try {
										using (var stream = File.OpenWrite(tmp))
											zipItem.Extract(stream);
										ImportOrder(userId, tmp);
									} finally {
										File.Delete(tmp);
									}
								} else {
									log.Error($"Не знаю как обработать файл {zipItem.FileName} в архиве {file}");
								}
							}
						}
						File.Delete(file);
					}
				} catch(Exception e) {
					log.Error($"Не удалось обработать файл {file}", e);
				}
			}
		}

		private static void ImportOrder(uint userId, string file)
		{
			using (var session = Factory.OpenSession())
			using (var trx = session.BeginTransaction()) {
				uint id;
				var log = new StringWriter();
				var rejects = new List<Reject>();
				var order = ParseOrder(session, userId, XDocument.Load(file), rejects, log, out id);
				if (order != null)
					session.Save(order);
				trx.Commit();
			}
		}

		private static IList<uint> GetAddressId(ISession session, string depId, string predId)
		{
			depId = depId.Slice(-2, -1);
			var addressIds = session.CreateSQLQuery(@"
select ai.AddressId
from Customers.Intersection i
	join Customers.AddressIntersection ai on ai.IntersectionId = i.Id
	join Usersettings.Pricesdata pd on pd.PriceCode = i.PriceId
		join Customers.Suppliers s on s.Id = pd.FirmCode
where i.SupplierClientId = :supplierClientId
	and ai.SupplierDeliveryId = :supplierDeliveryId
	and s.Id = :supplierId
group by ai.AddressId")
				.SetParameter("supplierClientId", predId)
				.SetParameter("supplierDeliveryId", depId)
				.SetParameter("supplierId", SupplierIdForCodeLookup)
				.List<uint>();
			if (addressIds.Count == 0) {
				throw new UserFriendlyException(
					$"Не удалось найти адрес доставки supplierClientId = {predId} supplierDeliveryId = {depId} игнорирую заказ",
					"Неизвестный отправитель");
			}
			return addressIds;
		}

		private static string TryGetUserFriendlyMessage(OrderException e)
		{
			var message = e.Message;
			if (e is UserFriendlyException) {
				message = ((UserFriendlyException)e).UserMessage;
			}
			return message;
		}

		public static Order ParseOrder(ISession session, uint userId, XDocument doc, List<Reject> rejects, TextWriter logForClient, out uint id)
		{
			var user = session.Load<User>(userId);
			id = 0;
			Order order = null;
			var packet = doc.XPathSelectElement("/PACKET");
			if (packet == null)
				return null;

			var type = (string)packet.Attribute("TYPE");
			if (type == null)
				return null;

			var predId = (string)packet.Attribute("PRED_ID");
			var to = (string)packet.Attribute("TO");
			var from = (string)packet.Attribute("FROM");
			var depId = (string)doc.XPathSelectElement("/PACKET/ORDER/DEP_ID");
			var orderElement = doc.XPathSelectElement("/PACKET/ORDER");
			var clientOrderId = SafeConvert.ToUInt32((string)orderElement.XPathSelectElement("ORDER_ID"));
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
				var qunatity = SafeConvert.ToUInt32((string)item.XPathSelectElement("QTTY"));
				var cost = Convert.ToDecimal((string)item.XPathSelectElement("PRICE"));
				var offerId = SafeConvert.ToUInt64((string)item.XPathSelectElement("XCODE"));
				var code = (string)item.XPathSelectElement("CODE");
				var name = (string)item.XPathSelectElement("NAME");

				reject.Items.Add(new RejectItem(clientOrderId, code, qunatity, name, cost, offerId));
			}
			rejects.Add(reject);

			var addressIds = GetAddressId(session, reject.DepId, reject.PredId);
			var address = session.Load<Address>(addressIds[0]);
			var rules = session.Load<OrderRules>(user.Client.Id);
			rules.Strict = false;
			rules.CheckAddressAccessibility = false;
			List<ActivePrice> activePrices;
			using(StorageProcedures.GetActivePrices((MySqlConnection)session.Connection, userId)) {
				activePrices = session.Query<ActivePrice>().ToList();
			}

			var comment = (string)orderElement.XPathSelectElement("COMMENT");
			var existOrder = session.Query<Order>().FirstOrDefault(o => o.UserId == userId && o.ClientOrderId == clientOrderId && !o.Deleted);
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
				}
				catch(OrderException e) {
					var message = TryGetUserFriendlyMessage(e);
					log.Warn($"Не удалось заказать позицию {item.Name} в количестве {item.Quantity}", e);
					logForClient.WriteLine("Не удалось заказать позицию {0} по заявке {3} в количестве {1}: {2}", item.Name, item.Quantity, message, clientOrderId);
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

		private static void ExportPrices(ISession session, uint userId, DirectoryInfo root)
		{
			var offers = QueryOffers(session, userId);
			foreach (var group in offers.GroupBy(o => o.PriceList)) {
				var activePrice = @group.Key;
				var name = Path.Combine(root.FullName, $"{activePrice.Id.Price.PriceCode}_{activePrice.Id.RegionCode}.xml");
				Export(name, w => ExportPrice(w, activePrice, group));
			}
		}

		private static void Export(string file, Action<XmlWriter> action)
		{
			var tmp = Path.GetTempFileName();
			try {
				var settings = new XmlWriterSettings { Encoding = Encoding.GetEncoding(1251) };
				using (var writer = XmlWriter.Create(tmp, settings)) {
					writer.WriteStartDocument(true);
					action(writer);
				}
				File.Move(tmp, file);
			} finally {
				File.Delete(tmp);
			}
		}

		private static IList<NamedOffer> QueryOffers(ISession session, uint userId)
		{
			var query = new OfferQuery();
			query.SelectSynonyms();

			using(StorageProcedures.GetActivePrices((MySqlConnection)session.Connection, userId)) {
				var sql = query.ToSql()
					.Replace(" as {Offer.Id.CoreId}", " as CoreId")
					.Replace(" as {Offer.Id.RegionCode}", " as RegionId")
					.Replace("{Offer.", "")
					.Replace("}", "");
				var offers = session.CreateSQLQuery(sql)
					.SetResultTransformer(new AliasToPropertyTransformer(typeof(NamedOffer)))
					.List<NamedOffer>();
				var activePrices = session.Query<ActivePrice>().Where(p => p.Id.Price.PriceCode > 0).ToList();
				offers.Each(offer => offer.PriceList = activePrices.First(price => price.Id.Price.PriceCode == offer.PriceCode && price.Id.RegionCode == offer.Id.RegionCode));
				return offers;
			}
		}

		public static void ExportPrice(XmlWriter writer, ActivePrice activePrice, IEnumerable<NamedOffer> offers)
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
	}
}
