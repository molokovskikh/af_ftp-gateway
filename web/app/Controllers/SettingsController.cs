using System.Linq;
using System.Web.Mvc;
using NHibernate;
using NHibernate.Linq;
using web_app.Config;
using web_app.Models;
using web_app.ViewModels;

namespace web_app.Controllers
{
	//управление пользователями
	[Authorize]
	[AdminActionAccess]
	public class SettingsController : BaseController
	{
		/// <summary>
		/// список сторонних пользователей
		/// </summary>
		public ActionResult UserList()
		{
			var items = DbSession.Query<Outsider>().OrderBy(x => x.Name).ToList();
			return View(items);
		}
		/// <summary>
		/// Добавление/редактирование стороннего пользователя
		/// </summary>
		[HttpGet]
		public ActionResult UserProfile(int? id)
		{
			var newOutsider = new OutsiderProfile();
			if (id.HasValue) {
				newOutsider = new OutsiderProfile(DbSession.Query<Outsider>().FirstOrDefault(s => s.Id == id.Value) ?? new Outsider());
			}
			return View(newOutsider);
		}
		/// <summary>
		/// Обновление учетной записи стороннего пользователя
		/// </summary>
		[HttpPost]
		public ActionResult UserProfile([Bind(Include = "Id,Login,UserName,Password,Enabled")] OutsiderProfile userProfile)
		{
			//проверяем заданный логин пользователя на уникальность
			if (userProfile.Id == 0 && (DbSession.Query<Outsider>().Count(s => s.Login == userProfile.Login) > 0
       || DbSession.Query<Admin>().Count(s => s.Login == userProfile.Login) > 0)) {
				MessageShow("Пользователь с подобный логином уже существует.");
				return View(userProfile);
			}
			if (ModelState.IsValid) {
				//получаем модель данных
				var outsider = userProfile.GetDbModel(DbSession);
				if (userProfile.Id == 0) {
					//todo: дополнительные действия при регистрации (если их не будет - УДАЛИТЬ)
					userProfile.Register(outsider);
					DbSession.Save(outsider);
				}
				else
				{
					//todo: дополнительные действия при обновлении (если их не будет - УДАЛИТЬ)
					userProfile.Update(outsider);
					DbSession.Save(outsider);
				}
				return RedirectToAction("UserList");
			}
			return View(userProfile);
		}
	}
}