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
		ISession DbSession;

		public ActionResult Index()
		{
			var items = DbSession.Query<Client>().Take(10).OrderBy(x => x.Name).ToList();
			return View(items);
		}
	}
}