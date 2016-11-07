using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using app.Models;
using Common.Models;
using Common.Models.Repositories;
using Common.MySql;
using Common.Tools;
using Ionic.Zip;
using log4net;
using MySql.Data.MySqlClient;
using NHibernate;
using NHibernate.Linq;

namespace app.Helpers
{
	/// <summary>
	/// Заявки
	/// - загружаются из xml/dbf файлов (импортируются)
	/// - парсятся
	/// - содержат предложения и отказы от них
	/// - обрабатываются с учетом актуальных данных в БД
	/// - сохраняются в БД
	/// </summary>
	public static class OrderHelper
	{
		private static ILog log = LogManager.GetLogger(typeof (OrderHelper));

		public static void ImportOrders(DirectoryInfo root, uint userId, ProtocolType ftpFileType)
		{
			var files = Directory.GetFiles(root.FullName);
			foreach (var file in files) {
				try {
					var ext = Path.GetExtension(file);
					if ((ext.Match(".xml") || ext.Match(".ord")) && ftpFileType == ProtocolType.Xml) {
						Protocols.Xml.OrderImport(userId, file);
						File.Delete(file);
					} else if (ext.Match(".zip") && ftpFileType == ProtocolType.Xml) {
						using (var zip = ZipFile.Read(file)) {
							foreach (var zipItem in zip) {
								if (Path.GetExtension(zipItem.FileName).Match(".xml")) {
									var tmp = Path.GetTempFileName();
									try {
										using (var stream = File.OpenWrite(tmp))
											zipItem.Extract(stream);
										Protocols.Xml.OrderImport(userId, tmp);
									} finally {
										File.Delete(tmp);
									}
								} else {
									log.Error($"Не знаю как обработать файл {zipItem.FileName} в архиве {file}");
								}
							}
						}
						File.Delete(file);
					} else if (ext.Match(".dbf") && ftpFileType == ProtocolType.Dbf) {
						Protocols.Dbf.OrderImport(userId, file);
						File.Delete(file);
					} else if (ext.Match(".dbf") && ftpFileType == ProtocolType.DbfAsna) {
						Protocols.DbfAsna.OrderImport(userId, file);
						File.Delete(file);
					}
				} catch (Exception e) {
					log.Error($"Не удалось обработать файл {file}", e);
				}
			}
		}
	}
}