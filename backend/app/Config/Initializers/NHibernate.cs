using System.Linq;
using System.Reflection;
using app.Models;
using Common.Models;
using Common.NHibernate;
using NHibernate.Mapping.Attributes;

namespace app.Config.Initializers
{
	public class NHibernate : BaseNHibernate
	{
		public override void Init()
		{
			Configuration.AddInputStream(HbmSerializer.Default.Serialize(Assembly.Load("Common.Models")));
			Mapper.Class<ArchiveOffer>(m => {
				m.Catalog("Farm");
				m.Table("CoreArchive");
				m.ManyToOne(i => i.PriceList, x => x.Column("PriceCode"));
			});
			Mapper.Class<Document>(m => {
				m.Catalog("Documents");
				m.Table("DocumentHeaders");
				m.OneToOne(i => i.Invoice, _ => { });
				m.ManyToOne(i => i.Supplier, x => x.Column("FirmCode"));
			});
			Mapper.Class<Invoice>(m => {
				m.Catalog("Documents");
				m.Table("InvoiceHeaders");
				m.OneToOne(i => i.Document, _ => { });
				m.Property(i => i.RecipientAddress, i => i.Column("ConsigneeInfo"));
			});
			Mapper.Class<DocumentLine>(m => {
				m.Catalog("Documents");
				m.Table("DocumentBodies");
				m.ManyToOne(i => i.ProductEntity, x => x.Column("ProductId"));
				m.Property(i => i.ProducerCostWithoutNDS, x => x.Column("ProducerCost"));
			});
			Include.Add(typeof(ArchiveOffer));
			base.Init();
		}
	}
}