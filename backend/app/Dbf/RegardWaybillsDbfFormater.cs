using app.Models;
using Common.Models;
using Common.Tools;
using NHibernate;
using System.Data;
using System.Linq;

namespace app.Dbf
{
	public class RegardWaybillsDbfFormater
	{
		public DataTable FillFormater(ISession session, Document document)
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

			foreach (var line in lines)
				{
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
	}
}
