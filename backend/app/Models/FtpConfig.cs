using System;
using Common.Models;

namespace app.Models
{
	public class FtpConfig
	{
		public FtpConfig()
		{
		}

		public FtpConfig(User user, Supplier supplier)
		{
			User = user;
			Supplier = supplier;
		}

		public virtual int Id { get; set; }
		public virtual User User { get; set; }
		public virtual Supplier Supplier { get; set; }
		public virtual string PriceUrl { get; set; }
		public virtual string WaybillUrl { get; set; }
		public virtual string OrderUrl { get; set; }
	}
}