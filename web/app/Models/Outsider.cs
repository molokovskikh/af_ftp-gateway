using System.Web;
using web_app.Interfaces;

namespace web_app.Models
{
	/// <summary>
	///сторонний пользователь
	/// </summary>
	public class Outsider : IWebOperator
	{
		public virtual uint Id { get; set; }

		public virtual string Login { get; set; }

		public virtual bool IsAdmin => false;

		public virtual string Name { get; set; }

		public virtual IWebOperator GetOperator()
		{
			return this;
		}
		public virtual bool Enabled { get; set; }

	}
}