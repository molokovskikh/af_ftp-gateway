using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Common.Tools;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Linq;
using web_app.Interfaces;
using web_app.Models;

namespace web_app.Controllers
{
	public class BaseController : Controller
	{
		//Сессия Хибера
		public ISession DbSession => (ISession)HttpContext.Items[typeof(ISession)];
		//Текущий пользователь
		public IWebOperator CurrentWebOperator => (IWebOperator)HttpContext.Items[typeof(IWebOperator)];


		protected override void OnActionExecuting(ActionExecutingContext context)
		{
			base.OnActionExecuting(context);
			//получаем текущий путь
			var messageRoute = GetCookie("MessageRoute");
			var currentRoute = context.RouteData.Values["controller"].ToString();
			currentRoute += "/" + context.RouteData.Values["action"];
			//если текущий путь соответствует сообщению, которое необходимо вывести на страницу
			if (!string.IsNullOrEmpty(messageRoute) && messageRoute == currentRoute) {
				//получаем содержимое из куков
				ViewBag.TopMessage = new Tuple<string, string>(GetCookie("MessageText"), GetCookie("MessageType"));
			}
		}

		protected override void OnActionExecuted(ActionExecutedContext context)
		{
			ViewBag.WebOperator = CurrentWebOperator;
		}

		/// <summary>
		/// Показать сообщение
		/// </summary>
		/// <param name="text">Текст</param>
		/// <param name="messageType">Тип</param>
		protected void MessageShow(string text, MessageType messageType = MessageType.danger, string currentRoute = "")
		{
			if (string.IsNullOrEmpty(currentRoute)) {
				currentRoute = RouteData.Values["controller"].ToString();
				currentRoute += "/" + RouteData.Values["action"];
			}
			SetCookie("MessageText", text);
			SetCookie("MessageType", messageType.ToString());
			SetCookie("MessageRoute", currentRoute);
			ViewBag.TopMessage = new Tuple<string, string>(text, messageType.ToString());
		}

		/// <summary>
		/// Получить куки
		/// </summary>
		/// <param name="cookieName">Название</param>
		/// <returns></returns>
		protected string GetCookie(string cookieName)
		{
			try {
				var cookie = Request.Cookies.Get(cookieName);
				var base64EncodedBytes = Convert.FromBase64String(cookie.Value);
				return Encoding.UTF8.GetString(base64EncodedBytes);
			}
			catch (Exception e) {
				return null;
			}
		}

		/// <summary>
		/// Добавить куки
		/// </summary>
		/// <param name="name">Название</param>
		/// <param name="value">Значение</param>
		public void SetCookie(string name, string value)
		{
			if (value == null) {
				Response.Cookies.Add(new HttpCookie(name, "false") { Path = "/", Expires = SystemTime.Now() });
				return;
			}
			var plainTextBytes = Encoding.UTF8.GetBytes(value);
			var text = Convert.ToBase64String(plainTextBytes);
			Response.Cookies.Add(new HttpCookie(name, text) { Path = "/" });
		}

		/// <summary>
		/// Удалить куки
		/// </summary>
		/// <param name="name">Название</param>
		public void DeleteCookie(string name)
		{
			Response.Cookies.Remove(name);
		}
	}

	/// <summary>
	/// Тип сообщения
	/// </summary>
	public enum MessageType
	{
		primary,
		success,
		info,
		warning,
		danger
	}
}