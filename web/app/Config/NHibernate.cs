using System.Collections.Generic;
using System.Configuration;
using Common.NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using web_app.Models;
using web_app.ViewModels;
using Configuration = System.Configuration.Configuration;

namespace web_app
{
	public class NHibernate : BaseNHibernate
	{
		public override void Init()
		{
			Excludes.Add(typeof(OutsiderProfile));
			Mapper.Class<Client>(x => x.Schema("Customers"));
			Mapper.Class<User>(x => { x.Schema("Customers"); });
			Mapper.Class<Outsider>(x => {
				x.Schema("Customers");
				x.Table("webftpoutsiders");
			});
			Mapper.Class<Admin>(x => {
				x.Schema("AccessRight");
				x.Table("RegionalAdmins");
				x.Id(y => y.Id, y => y.Column("RowId"));
				x.Property(o => o.Login, om => om.Column("UserName"));
				x.Property(o => o.Name, om => om.Column("ManagerName"));
			});
			base.Init();
		}
	}
}