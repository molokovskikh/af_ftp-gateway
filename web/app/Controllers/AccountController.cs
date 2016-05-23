using System.Web.Mvc;
using System.Web.Security;
using web_app.Models;

namespace web_app.Controllers
{
	public class AccountController : Controller
	{
		public ActionResult Login()
		{
			var model = new Login();
			if (Request.HttpMethod == "POST") {

				if (TryUpdateModel(model)) {
					FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
					return RedirectToAction("Index", "Client");
				}
			}
			return View(model);
		}

		public void Logoff()
		{
			FormsAuthentication.SignOut();
			RedirectToAction("Login");
		}
	}
}