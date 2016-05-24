using System.Linq;
using System.Web.Mvc;
using NHibernate;
using NHibernate.Linq;
using web_app.Models;

namespace web_app.Controllers
{
	[Authorize]
	public class ClientController : Controller
	{
		public ISession DbSession => (ISession)HttpContext.Items[typeof(ISession)];

		public ActionResult Index()
		{
			var items = DbSession.Query<Client>().OrderBy(x => x.Name).Take(10).ToList();
			return View(items);
		}
	}
}