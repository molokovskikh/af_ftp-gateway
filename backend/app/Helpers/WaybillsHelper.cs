﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using app.Models;
using Common.Models;
using Common.Tools;
using NHibernate;

namespace app.Helpers
{
	/// <summary>
	/// Накладные
	/// - сохраняются в xml/dbf (зависит от флага "ftpFileType") файлы (экспортируются)
	/// -	берутся из таблицы "Documents", предполагается что документ типа "Накладная" (?)
	///
	/// </summary>
	class WaybillsHelper
	{
		public static void ExportWaybills(DirectoryInfo root, ISession session, uint userId, ProtocolType ftpFileType)
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
				var doc = session.Load<Document>(id);
				if (ftpFileType == ProtocolType.Xml) {
					var name = Path.Combine(root.FullName, id + ".xml");
					Protocols.Xml.SaveInFile(name, t => Protocols.Xml.FormatterRegardWaybillsExport(session, t, doc));
				} else if (ftpFileType == ProtocolType.Dbf) {
					var name = Path.Combine(root.FullName, id + ".dbf");
					Protocols.Dbf.SaveInFile(name, Protocols.Dbf.FormatterRegardWaybillsExport(session, doc));
				} else if (ftpFileType == ProtocolType.DbfAsna) {
					var name = Path.Combine(root.FullName, id + ".dbf");
					Protocols.DbfAsna.SaveInFile(name, Protocols.DbfAsna.FormatterRegardWaybillsExport(session, doc));
				}
			}
		}
	}
}