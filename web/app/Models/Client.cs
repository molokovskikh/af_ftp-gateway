using System.Collections.Generic;

namespace web_app.Models
{
	/// <summary>
	/// клиент
	/// </summary>
	public class Client
	{
		public virtual uint Id { get; set; }

		public virtual string Name { get; set; }

		public virtual string FullName { get; set; }

		public virtual bool FtpIntegration { get; set; }
	}
}