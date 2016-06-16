using app.Models;
using Common.Models;
using Common.Tools;
using System.Collections.Generic;
using System.Data;

namespace app.Dbf
{
	public class RegardRejectDbfFormater
	{

		// заготовка TODO
		public DataTable FillFormater(List<Reject> rejects)
		{

			var table = new DbfTable();
			table.Columns(
				Column.Numeric("NUMZ", 8),
				Column.Date("DATEZ"),
				Column.Char("CODEPST", 12),
				Column.Date("DATE"),
				Column.Char("PODR", 40),
				Column.Numeric("QNT", 8),
				Column.Numeric("QNTO", 8),
				Column.Numeric("PRICE", 9, 2),
				Column.Char("PODRCD", 12)
			);

			//foreach (var reject in rejects)
			//{
			//	table.Row(
			//		Value.For("NUMZ", reject.NUMZ),
			//		Value.For("DATEZ", reject.DATEZ),
			//		Value.For("CODEPST", reject.CODEPST),
			//		Value.For("DATE", reject.DATE),
			//		Value.For("PODR", reject.PODR),
			//		Value.For("QNT", reject.QNT),
			//		Value.For("QNTO", reject.QNTO),
			//		Value.For("PRICE", reject.PRICE),
			//		Value.For("PODRCD", reject.PODRCD)
			//	);
			//}
			return table.ToDataTable();
		}
	}
}
