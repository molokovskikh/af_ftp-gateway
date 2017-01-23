using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.UI.WebControls;
using app.Helpers;
using app.Protocols;
using Common.Tools;
using FluentFTP;
using log4net;
using NHibernate;
using NHibernate.Linq;
using Quartz;

namespace app.Models
{
	public class FtpExportJob : IJob
	{
		private static ILog log = LogManager.GetLogger(typeof(FtpExportJob));

		public void Execute(IJobExecutionContext context)
		{
			using (var session = Program.Factory.OpenSession()) {
				var users = FtpUsers(session);
				foreach (var userId in users)
					using (var userSession = Program.Factory.OpenSession()) {
						try {
							ProcessUser(userSession, userId);
						} catch(Exception e) {
							log.Error($"Ошибка при обработке пользователя {userId}", e);
#if DEBUG
							throw;
#endif
						}
					}
			}
		}

		private void ProcessUser(ISession session, uint userId)
		{
			log.Debug($"Обработка пользователя {userId}");
			IList<NamedOffer> offers;
			using (var trx = session.BeginTransaction()) {
				offers = PriceHelper.QueryOffers(session, userId);
				trx.Commit();
			}
			var config = session.Query<FtpConfig>().Where(x => x.User.Id == userId).ToList();
			foreach (var supplierConfig in config) {
				using (var cleander = new FileCleaner())
				using (var client = new FtpClient()) {
					var url = supplierConfig.PriceUrl;
					try {
						if (!String.IsNullOrEmpty(url)) {
							OpenFtp(new Uri(url), client);
							var priceOffers = offers.Where(x => x.PriceList.Id.Price.Supplier.Id == supplierConfig.Supplier.Id)
								.ToArray();
							var tmp = cleander.TmpFile();
							Dbf2.SaveAsDbf4(DbfAsna.Price(priceOffers), tmp);

							UploadFile(client, tmp, new Uri(url));
						}
					} catch(Exception e) {
						log.Error($"Не удалось выгрузить прайс-лист поставщика {supplierConfig.Supplier.Name} ({supplierConfig.Supplier.Id}) в {url}", e);
#if DEBUG
						throw;
#endif
					}

					url = supplierConfig.WaybillUrl;
					if (!String.IsNullOrEmpty(url)) {
						try {
							var waybillIds = session.CreateSQLQuery(@"
select dh.Id, d.RowId, ds.Id as SendLogId
from Logs.DocumentSendLogs ds
	join Logs.Document_logs d on d.RowId = ds.DocumentId
	join Documents.DocumentHeaders dh on dh.DownloadId = d.RowId
where ds.UserId = :userId
	and ds.Committed in (0, 2)
	and d.FirmCode = :supplierId
order by d.LogTime desc
limit 400;")
								.SetParameter("userId", userId)
								.SetParameter("supplierId", supplierConfig.Supplier.Id)
								.List<object[]>();
							foreach (var pair in waybillIds) {
								var doc = session.Load<Document>(Convert.ToUInt32(pair[0]));
								var tmp = cleander.TmpFile();
								Dbf2.SaveAsDbf4(DbfAsna.Waybill(session, doc), tmp);
								var part = url;
								if (!part.EndsWith("/"))
									part += "/";
								UploadFile(client, tmp, new Uri(part + pair[1] + ".dbf"));
								using (var trx = session.BeginTransaction()) {
									var sendLog = session.Load<DocumentSendLog>(Convert.ToUInt32(pair[2]));
									sendLog.Commit();
									session.Flush();
									trx.Commit();
								}
							}
						} catch(Exception e) {
							log.Error($"Не удалось выгрузить накладную поставщика {supplierConfig.Supplier.Name} ({supplierConfig.Supplier.Id}) в {url}", e);
#if DEBUG
							throw;
#endif
						}

					}
				}
			}
		}

		public static void UploadFile(FtpClient client, string filename, Uri url)
		{
			var path = url.GetComponents(UriComponents.Path, UriFormat.Unescaped);
			var dir = Path.GetDirectoryName(path);
			if (!client.DirectoryExists(dir)) {
				client.CreateDirectory(dir);
			}
			using (var dst = client.OpenWrite(path, FtpDataType.Binary))
			using (var src = File.OpenRead(filename))
				src.CopyTo(dst);
			log.Info($"Выгружен файл {url} размер {new FileInfo(filename).Length}");
		}

		public static void OpenFtp(Uri url, FtpClient client)
		{
			client.Host = url.Host;
			if (!url.IsDefaultPort)
				client.Port = url.Port;
			if (!string.IsNullOrEmpty(url.UserInfo)) {
				var parts = url.UserInfo.Split(':');
				client.Credentials = new NetworkCredential(parts[0], parts.LastOrDefault() ?? "");
			} else {
				client.Credentials = new NetworkCredential("anonymous", "anonymous@localhost.local");
			}

			client.Connect();
		}

		public static IList<uint> FtpUsers(ISession session)
		{
			var userIds = session.CreateSQLQuery("select Id from Customers.Users where UseFtpGateway = 1 and FtpFileType = :type")
				.SetParameter("type", (int) ProtocolType.DbfAsna)
				.List<uint>();
			return userIds;
		}
	}
}