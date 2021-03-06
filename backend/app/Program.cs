﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Common.Models;
using Common.Tools;
using Common.Tools.Threading;
using log4net;
using log4net.Config;
using NHibernate;
using app.Helpers;
using app.Models;
using Quartz;
using Quartz.Impl;

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
			var scheduler = StdSchedulerFactory.GetDefaultScheduler();
			CronJob(scheduler, config.FtpExportPlan, typeof(FtpExportJob));
			CronJob(scheduler, config.FtpImportPlan, typeof(FtpImportJob));
			scheduler.Start();
			token.Register(() => scheduler.Shutdown());

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
						var ftpFileType = (ProtocolType) Convert.ToInt16(user[1]);
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

		private static void CronJob(IScheduler scheduler, string plan, Type type)
		{
			var job = JobBuilder.Create(type).WithIdentity(type.Name).Build();
			var trigger = TriggerBuilder.Create()
				.WithIdentity(type.Name + "Trigger")
				.WithCronSchedule(plan)
				.Build();
			scheduler.ScheduleJob(job, trigger);
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

		public static string GetSupplierDeliveryId(ISession session, Document document)
		{
			return session.CreateSQLQuery(@"
select ai.SupplierDeliveryId
from Customers.Intersection i
	join Customers.AddressIntersection ai on ai.IntersectionId = i.Id
	join Usersettings.Pricesdata pd on pd.PriceCode = i.PriceId
		join Customers.Suppliers s on s.Id = pd.FirmCode
where i.SupplierClientId <=> null
	and ai.AddressId = :addressId
	and i.ClientId = :clientId
	and s.Id = :supplierId
group by ai.AddressId")
				.SetParameter("addressId", document.Address.Id)
				.SetParameter("supplierId", Program.SupplierIdForCodeLookup)
				.SetParameter("clientId", document.ClientCode)
				.List<string>()
				.FirstOrDefault();
		}

		public static IList<uint> GetAddressId(ISession session, string supplierDeliveryId, string supplierClientId,
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
		join Customers.Addresses a on a.Id = ai.AddressId
	join Usersettings.Pricesdata pd on pd.PriceCode = i.PriceId
		join Customers.Suppliers s on s.Id = pd.FirmCode
where i.SupplierClientId <=> :supplierClientId
	and ai.SupplierDeliveryId <=> :supplierDeliveryId
	and i.ClientId = :clientId
	and s.Id = :supplierId
	and a.Enabled = 1
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