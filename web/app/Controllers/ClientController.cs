using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using NHibernate;
using NHibernate.Linq;
using web_app.Models;
using web_app.Services;
using System.Configuration;
using web_app.Helper;

namespace web_app.Controllers
{
	//управление найстройками клиента(ов)
	[Authorize]
	public class ClientController : BaseController
	{
		public ISession DbSession => (ISession)HttpContext.Items[typeof(ISession)];

		public ActionResult Index(int currentPage = 1, string search = "")
		{
			//определяем кол-во записей в результате поиска
			var count = search == string.Empty
				? DbSession.Query<Client>().Count(s => s.FtpIntegration)
				: DbSession.Query<Client>().Count(s => s.FtpIntegration && SqlMethods.Like(s.Name, $"%{search.Trim()}%"));
			//получаем листалку для рассчета объемов выборки
			var items = new Paginator<Client>(count, currentPage);
			//получаем список пользователей по результату выборки
			if (search == string.Empty) {
				items.SetList(DbSession.Query<Client>().Where(s => s.FtpIntegration).OrderBy(x => x.Name).Skip(items.ItemCurrent).Take(items.ItemsPerPage).ToList());
			}
			else {
				items.SetList(DbSession.Query<Client>().Where(s => s.FtpIntegration && (SqlMethods.Like(s.Name, $"%{search.Trim()}%") || SqlMethods.Like(s.FullName, $"%{search.Trim()}%")))
					.OrderBy(x => x.Name).Skip(items.ItemCurrent).Take(items.ItemsPerPage).ToList());
			}
			ViewBag.SearchPhrase = search;
			//передаем на форму листалку
			return View(items);
		}

		/// <summary>
		/// Страница с информацией о клиенте, с возможностью его FTP интеграции (создание User, выдача логина и пароля от FTP)
		/// </summary>
		public ActionResult Info(int id)
		{
			var item = DbSession.Query<Client>().FirstOrDefault(x => x.Id == id);
			ViewBag.UserList = DbSession.Query<User>().Where(s => s.ClientId == id && s.UseFtpGateway).OrderBy(x => x.Name).ToList();
			return View(item);
		}

		/// <summary>
		/// Включение FTP интеграции, создание сторонним приложением нового пользователя (ftp, т.есть User), получение его логина и пароля
		/// </summary>
		public ActionResult SwitchOnIntegration(uint clientId)
		{
			var item = DbSession.Query<Client>().FirstOrDefault(x => x.Id == clientId);
			if (item == null) {
				MessageShow("Данного клиента не существует");
				return RedirectToAction("Index");
			}
			var htmlResult = new string[0];
#if DEBUG
			htmlResult = new[] { "newLogin", "newPass" };
			var user = DbSession.Query<web_app.Models.User>().OrderByDescending(s => s.Id).FirstOrDefault();
			user.ClientId = clientId;
			user.UseFtpGateway = true;
			DbSession.Save(user);
#else

			//запрос на добавление пользователя к другому приложению
			string ulrNewUser = ConfigurationManager.AppSettings["UpdateClientFtpState"];
			//отправка запроса на добавление пользователя
			//получение строки с логином и паролем в случае удачной авторизации
			string parametres = $"id={item.Id}";


			var serviceConnection = new AdminServiceConnection("analit");
			var tt = serviceConnection.ExecuteAction<dynamic>(ulrNewUser, parametres);
			htmlResult = tt.Result.ToString().Split(',');
#endif
			//если логин и пароль есть, отображаем их в сообщении
			if (!string.IsNullOrEmpty(htmlResult[0]) && !string.IsNullOrEmpty(htmlResult[1])) {
				MessageShow($"Ваш <strong>логин</strong>: {htmlResult[0]}, <strong>пароль</strong>: {htmlResult[1]}", MessageType.success, "Client/Info");
			}
			return RedirectToAction("Info", new { id = clientId });
		}

		public ActionResult EditSettings(uint userId, int ftpFileType)
		{
			var user = DbSession.Query<User>().FirstOrDefault(x => x.Id == userId);
			if (user == null)
			{
				MessageShow("Данного пользователя не существует");
				return RedirectToAction("Index");
			}
			user.FtpFileType = ftpFileType;
			DbSession.Save(user);
			return RedirectToAction("Info", new { id = user.ClientId });
		}
	}
}