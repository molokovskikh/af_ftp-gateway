using System.Collections.Generic;
using System.Web;
using web_app.Interfaces;

namespace web_app.Models
{
	/// <summary>
	/// Пользователь ftp
	/// </summary>
	public class User
	{
		public virtual uint Id { get; set; }

		public virtual string Login { get; set; }

		public virtual bool IsAdmin => false;

		public virtual string Name { get; set; }

		public virtual bool Enabled { get; set; }

		public virtual uint ClientId { get; set; }

		public virtual bool UseFtpGateway { get; set; }

	}
}