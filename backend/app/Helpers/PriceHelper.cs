using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Common.Models;
using Common.Models.Repositories;
using Common.MySql;
using Common.NHibernate;
using Common.Tools;
using Common.Tools.Helpers;
using MySql.Data.MySqlClient;
using NHibernate;
using NHibernate.Linq;

namespace app.Helpers
{
	/// <summary>
	/// Прайс-листы
	/// - сохраняются в xml/dbf файлы (экспортируются)
	/// - обрабатываются на основе предложений пользователей
	/// - тело прайся является списком предложений
	/// </summary>
	public static class PriceHelper
	{
		public static void ExportPrices(ISession session, uint userId, DirectoryInfo root, ProtocolType ftpFileType)
		{
			var offers = QueryOffers(session, userId);
			foreach (var group in offers.GroupBy(o => o.PriceList)) {
				var activePrice = @group.Key;
				if (ftpFileType == ProtocolType.Xml) {
					var name = Path.Combine(root.FullName, $"{activePrice.Id.Price.PriceCode}_{activePrice.Id.RegionCode}.xml");
					Protocols.Xml.SaveInFile(name, t => Protocols.Xml.FormatterRegardPricesExport(t, activePrice, group));
				} else if (ftpFileType == ProtocolType.Dbf) {
					var name = Path.Combine(root.FullName, $"{activePrice.Id.Price.PriceCode}_{activePrice.Id.RegionCode}.dbf");
					Protocols.Dbf.SaveInFile(name, Protocols.Dbf.FormatterRegardPricesExport(activePrice, group));
				} else if (ftpFileType == ProtocolType.DbfAsna) {
					var name = Path.Combine(root.FullName, $"{activePrice.Id.Price.PriceCode}_{activePrice.Id.RegionCode}.dbf");
					Protocols.DbfAsna.SaveInFile(name, Protocols.DbfAsna.FormatterRegardPricesExport(activePrice, group));
				}
			}
		}

		private static IList<NamedOffer> QueryOffers(ISession session, uint userId)
		{
			var query = new OfferQuery();
			query.SelectSynonyms();

			using (StorageProcedures.GetActivePrices((MySqlConnection) session.Connection, userId)) {
				var sql = query.ToSql()
					.Replace(" as {Offer.Id.CoreId}", " as CoreId")
					.Replace(" as {Offer.Id.RegionCode}", " as RegionId")
					.Replace("{Offer.", "")
					.Replace("}", "");
				var offers = session.CreateSQLQuery(sql)
					.SetResultTransformer(new AliasToPropertyTransformer(typeof (NamedOffer)))
					.List<NamedOffer>();
				var activePrices = session.Query<ActivePrice>().Where(p => p.Id.Price.PriceCode > 0).ToList();
				offers.Each(
					offer =>
						offer.PriceList =
							activePrices.First(
								price => price.Id.Price.PriceCode == offer.PriceCode && price.Id.RegionCode == offer.Id.RegionCode));
				return offers;
			}
		}
	}
}