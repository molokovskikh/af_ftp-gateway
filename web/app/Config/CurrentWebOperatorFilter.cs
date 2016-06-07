using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using NHibernate;
using NHibernate.Linq;
using web_app.Interfaces;
using web_app.Models;

namespace web_app.Config
{
	public class CurrentWebOperatorFilter : ActionFilterAttribute
	{
		public ISession DbSession => (ISession)HttpContext.Current.Items[typeof(ISession)];

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			IWebOperator currentWebOperator = null;
			var isAdmin = false;
			if (!string.IsNullOrEmpty(filterContext.HttpContext.User.Identity.Name)) {
				if (filterContext.Controller.ControllerContext.RequestContext.HttpContext.Request.IsAuthenticated) {
					var cookie = filterContext.Controller.ControllerContext
						.RequestContext
						.HttpContext
						.Request
						.Cookies[FormsAuthentication.FormsCookieName];
					if (cookie == null)
						return;
					var decrypted = FormsAuthentication.Decrypt(cookie.Value);
					if (decrypted != null)
						bool.TryParse(decrypted.UserData, out isAdmin);
				}
			}
			if (isAdmin) {
				currentWebOperator = DbSession.Query<Admin>().FirstOrDefault(s => s.Login == filterContext.HttpContext.User.Identity.Name);
			}
			else {
				currentWebOperator = DbSession.Query<Outsider>().FirstOrDefault(s => s.Login == filterContext.HttpContext.User.Identity.Name);
				if (currentWebOperator != null && (filterContext.Controller.GetType().GetCustomAttributes(typeof(AdminActionAccess), true).Length > 0
					|| filterContext.ActionDescriptor.GetCustomAttributes(typeof(AdminActionAccess), true).Length > 0)) {
					filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Account" }, { "action", "Login" } });
				}
			}
			filterContext.HttpContext.Items[typeof(IWebOperator)] = currentWebOperator;
		}
	}
}