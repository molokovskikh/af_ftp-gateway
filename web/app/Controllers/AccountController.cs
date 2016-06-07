using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.ModelBinding;
using System.Web.Mvc;
using System.Web.Security;
using Common.Tools;
using NHibernate;
using NHibernate.Linq;
using web_app.Interfaces;
using web_app.Models;
using web_app.Services;
using web_app.ViewModels;

namespace web_app.Controllers
{
	/// <summary>
	/// Аутентификация пользователей
	/// </summary>
	public class AccountController : BaseController
	{

		[HttpGet]
		public ActionResult Login()
		{
			if (CurrentWebOperator != null) {
				return RedirectToAction("Index", "Client");
			}
			return View(new Login());
		}

		[HttpPost]
		public ActionResult Login([Bind(Include = "UserName,Password,RememberMe")] Login login, string captcha)
		{
			if (CurrentWebOperator != null) {
				return RedirectToAction("Index", "Client");
			}
			if (ModelState.IsValid) {
				return Authentication(login);
			}
			return View(login);
		}

		public ActionResult Logoff()
		{
			FormsAuthentication.SignOut();
			Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, "false") { Path = "/", Expires = SystemTime.Now() });
			return RedirectToAction("Login");
		}

		/// <summary>
		/// Аутентификация по MS ActiveDirectory
		/// </summary>
		/// <param name="login">Логин и пароль</param>
		/// <returns></returns>
		private ActionResult Authentication(Login login)
		{
			string AuthorizedController = "Client";
			string AuthorizedAction = "Index";

			//попытка аутентифицировать пользователя по его логину и паролю
#if DEBUG
			var defaultPassword = ConfigurationManager.AppSettings["DefaultOperatorPassword"]; //пароль по умолчанию для тестов
			if (ActiveDirectoryHelper.IsAuthenticated(login.UserName, login.Password) || login.Password == defaultPassword) {
#else
			if (ActiveDirectoryHelper.IsAuthenticated(username, password)) {
#endif
				//поиск аутентифицированного пользователя в таблице региональных админов
				IWebOperator webOperator = DbSession.Query<Admin>().FirstOrDefault(p => p.Login == login.UserName);
				if (webOperator != null) {
					return Authenticate(AuthorizedAction, AuthorizedController, login.UserName, login.RememberMe, true.ToString());
				}
				//если там его нет, ищем в таблице сторонних пользователей
				else {
					webOperator = DbSession.Query<Outsider>().FirstOrDefault(p => p.Login == login.UserName && p.Enabled);
					if (webOperator != null) {
						return Authenticate(AuthorizedAction, AuthorizedController, login.UserName, login.RememberMe, false.ToString());
					}
					else {
						Logoff();
					}
				}
			}
			//если логин и пароль неверны возвращаем его на страницу ввода учетных данных
			MessageShow("Учетные данные введены неверно!", MessageType.danger);
			return RedirectToAction("Login");
		}

		/// <summary>
		/// Создание билета и куки для авторизованного пользователя
		/// </summary>
		/// <returns></returns>
		private ActionResult Authenticate(string action, string controller, string username, bool shouldRemember,
			string userData = "")
		{
			var ticket = new FormsAuthenticationTicket(
				1,
				username,
				SystemTime.Now(),
				SystemTime.Now().AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
				shouldRemember,
				userData,
				FormsAuthentication.FormsCookiePath);
			var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
			if (shouldRemember)
				cookie.Expires = SystemTime.Now().AddMinutes(FormsAuthentication.Timeout.TotalMinutes);
			else
				cookie.Expires = SystemTime.Now().AddMinutes(120);
			Response.Cookies.Set(cookie);
			return RedirectToAction(action, controller);
		}

	}
}