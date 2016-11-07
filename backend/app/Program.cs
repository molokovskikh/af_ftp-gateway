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
using Common.Tools.Helpers;
using Common.Tools.Threading;
using Ionic.Zip;
using log4net;
using log4net.Config;
using MySql.Data.MySqlClient;
using NHibernate;
using NHibernate.Linq;
using System.Data;
using app.Helpers;
using Order = Common.Models.Order;

namespace app
{
	public enum ProtocolType
	{
		Xml = 0,
		Dbf = 1,
		DbfAsna = 2
	}

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
				value = (bool) value ? 1 : 0;
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

		private static ILog log = LogManager.GetLogger(typeof (Program));

		public static int Main(string[] args)
		{
			CancellationTokenSource stop = null;
			try {
				AppDomain.CurrentDomain.UnhandledException +=
					(sender, eventArgs) => { log.Fatal("Необработанная ошибка", eventArgs.ExceptionObject as Exception); };
				TaskScheduler.UnobservedTaskException +=
					(sender, eventArgs) => { log.Fatal("Необработанная ошибка", eventArgs.Exception); };

				XmlConfigurator.Configure();
				GlobalContext.Properties["Version"] = typeof (Program).Assembly.GetName().Version;

				var config = new Config.Config(ConfigurationManager.AppSettings);
				stop = new CancellationTokenSource();
				SupplierIdForCodeLookup = config.SupplierId;

				var nHibernate = new Config.Initializers.NHibernate();
				nHibernate.Init();
				Factory = nHibernate.Factory;

				return CommandService.Start(args, stop, new Task(() => MainLoop(config, stop.Token)));
			} catch (Exception e) {
				log.Fatal("Не удалось запустить приложение", e);
				//если протоколирование сломано
				Console.Error.WriteLine(e);
				stop?.Cancel();
				return 1;
			}
		}

		public static void MainLoop(Config.Config config, CancellationToken token)
		{
			while (!token.IsCancellationRequested) {
				try {
					var logger = new MemorableLogger(log);
					IList<object[]> users;
					using (var session = Factory.OpenSession()) {
						users = session.CreateSQLQuery("select Id, FtpFileType from Customers.Users where UseFtpGateway = 1")
							.List<object[]>();
					}
					foreach (var user in users) {
						var userId = (uint) user[0];
						var ftpFileType = (ProtocolType) user[1];
						try {
							log.Debug($"Обработка пользователя {userId}");
							token.ThrowIfCancellationRequested();
							ProcessUser(config, userId, ftpFileType);
							logger.Forget(userId);
						} catch (Exception e) {
							if (e is OperationCanceledException)
								return;
							logger.Error($"Ошибка при обработке пользователя {userId}", e, userId);
						}
					}
					token.WaitHandle.WaitOne(config.LookupTime);
					token.ThrowIfCancellationRequested();
				} catch (Exception e) {
					if (e is OperationCanceledException)
						return;
					log.Error("Ошибка при обработке", e);
				}
			}
		}

		public static void ProcessUser(Config.Config config, uint userId, ProtocolType ftpFileType)
		{
			var userRoot = Path.Combine(config.RootDir, userId.ToString());
			var pricesDir = Directory.CreateDirectory(Path.Combine(userRoot, "prices"));
			var marker = pricesDir.EnumerateFiles("request.txt").FirstOrDefault();
			if (marker != null) {
				using (var session = Factory.OpenSession())
				using (var trx = session.BeginTransaction()) {
					PriceHelper.ExportPrices(session, userId, pricesDir, ftpFileType);
					trx.Commit();
				}
				marker.Delete();
			}

			var ordersDir = Directory.CreateDirectory(Path.Combine(userRoot, "orders"));
			OrderHelper.ImportOrders(ordersDir, userId, ftpFileType);

			var waybillsDir = Directory.CreateDirectory(Path.Combine(userRoot, "waybills"));
			using (var session = Factory.OpenSession())
			using (var trx = session.BeginTransaction()) {
				WaybillsHelper.ExportWaybills(waybillsDir, session, userId, ftpFileType);
				trx.Commit();
			}
		}
	}
}