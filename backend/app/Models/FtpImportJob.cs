using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Tools;
using FluentFTP;
using NHibernate;
using NHibernate.Linq;
using Quartz;
using log4net;

namespace app.Models
{
	public class FtpImportJob : IJob
	{
		static ILog log = LogManager.GetLogger(typeof(FtpImportJob));

		public void Execute(IJobExecutionContext context)
		{
			using (var session = Program.Factory.OpenSession()) {
				var userIds = FtpExportJob.FtpUsers(session);
				foreach (var userId in userIds) {
					try {
						using (var userSession = Program.Factory.OpenSession())
							ProcessUser(userSession, userId);
					} catch(Exception e) {
						log.Error($"Не удалось обработать пользователя {userId}", e);
#if DEBUG
						throw;
#endif
					}
				}
			}
		}

		private static void ProcessUser(ISession session, uint userId)
		{
			log.Debug($"Обработка пользователя {userId}");
			var config = session.Query<FtpConfig>().Where(x => x.User.Id == userId).ToList();
			foreach (var priceConfig in config) {
				using (var cleaner = new FileCleaner())
				using (var client = new FtpClient()) {
					var url = new Uri(priceConfig.OrderUrl);
					FtpExportJob.OpenFtp(url, client);
					var dir = url.GetComponents(UriComponents.Path, UriFormat.Unescaped);
					var files = client.GetListing(dir);
					foreach (var file in files) {
						var tmp = cleaner.TmpFile();
						var targetFile = Path.Combine(dir, file.Name);
						using (var dst = File.OpenWrite(tmp))
						using (var src = client.OpenRead(targetFile))
								src.CopyTo(dst);

						try {
							using (var trx = session.BeginTransaction()) {
								Protocols.DbfAsna.OrderImport(userId, tmp);
								client.DeleteFile(targetFile);
								trx.Commit();
							}
						} catch (Exception e) {
							log.Error($"Не удалось обработать файл {file.Name} из {url}", e);
#if DEBUG
							throw;
#endif
						}
					}
				}
			}
		}
	}
}