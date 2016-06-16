﻿using Common.Models;
using Common.Tools;
using System.Collections.Generic;
using System.Data;

namespace app.Dbf
{
	public class RegardPricesDbfFormater
	{
		public DataTable FillFormater(ActivePrice activePrice, IEnumerable<NamedOffer> offers)
		{

			//var supplier = activePrice.Id.Price.Supplier;
			//writer.WriteAttributeString("FROM", supplier.Name);
			//writer.WriteAttributeString("DATE", activePrice.PriceDate.ToString("dd.MM.yyyy HH:mm"));
			//writer.WriteAttributeString("NAME", $"{supplier.Name} {activePrice.Id.Price.PriceName}");

			//writer.Element("CODE", String.Concat(offer.Code, offer.CodeCr));
			//writer.Element("ACODE", offer.ProductId);
			//writer.Element("ACODECR", offer.CodeFirmCr);
			//writer.Element("XCODE", offer.Id.CoreId);

			var table = new DbfTable();
				table.Columns(
					Column.Char("CODEPST", 12),
					Column.Char("NAME", 80),
					Column.Char("CNTR", 15),
					Column.Char("FIRM", 40),
					Column.Numeric("QNTPACK", 8),
					Column.Char("EAN13", 13),
					Column.Numeric("NDS", 9, 2),
					Column.Date("GDATE"),
					Column.Numeric("QNT", 9, 2),
					Column.Numeric("NSP", 9, 2),
					Column.Numeric("GNVLS", 1),
					Column.Numeric("PRICE1", 9, 2),
					Column.Numeric("NEWFLG", 1),
					Column.Numeric("PROTECID", 8),
					Column.Numeric("QNTPST", 8));

				foreach (var offer in offers)
				{
					table.Row(
						Value.For("CODEPST", offer.Code),
						Value.For("NAME", offer.ProductSynonym),
						//Value.For("CNTR", offer.),
						Value.For("FIRM", offer.ProducerSynonym),
						Value.For("QNTPACK", offer.RequestRatio),
						Value.For("EAN13", offer.EAN13.Slice(12)),
						//Value.For("NDS", offer.),
						Value.For("GDATE", offer.NormalizedPeriod), // !
						Value.For("QNT", offer.Quantity),
						//Value.For("NSP", offer.),
						Value.For("GNVLS", offer.VitallyImportant),
						Value.For("PRICE1", offer.Cost) // Базовая
						//Value.For("NEWFLG", offer.PriceDate)
						//Value.For("PROTECID", offer.PriceDate)
						//Value.For("QNTPST", offer.)
					);
				}
				return table.ToDataTable();
		}
	}
}
