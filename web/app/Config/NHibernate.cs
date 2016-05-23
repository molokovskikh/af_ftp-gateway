using System.Collections.Generic;
using System.Configuration;
using Common.NHibernate;
using NHibernate.Cfg;
using web_app.Models;
using Configuration = System.Configuration.Configuration;

namespace web_app
{
	public class NHibernate : BaseNHibernate
	{
		public override void Init()
		{
			base.Init();

			Mapper.Class<Client>(x => x.Schema("Customers"));
			Mapper.Class<User>(x => x.Schema("Customers"));
			Mapper.Class<Admin>(x => {
				x.Schema("AccessRight");
				x.Table("RegionalAdmins");
				x.Id(y => y.Id, y => y.Column("RowId"));
			});
		}
	}
}