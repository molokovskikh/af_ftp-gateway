using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using log4net;
using NHibernate;
using LogManager = WebGrease.LogManager;

namespace web_app
{
	public class SessionFilter : ActionFilterAttribute
	{
		private ILog log = log4net.LogManager.GetLogger(typeof(SessionFilter));
		private ISessionFactory factory;

		public SessionFilter(ISessionFactory factory)
		{
			this.factory = factory;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var controller = context.Controller;
			if (controller == null || controller.GetType().GetProperty("Session") == null)
				return;

			var session = factory.OpenSession();
			try {
				session.BeginTransaction();
				((dynamic)context.Controller).Session = session;
			}
			catch {
				session.Close();
				throw;
			}
		}

		public override void OnActionExecuted(ActionExecutedContext context)
		{
			var controller = context.Controller;
			if (controller == null || controller.GetType().GetProperty("Session") == null)
				return;
			var session = (ISession)((dynamic)controller).Session;
			if (session == null)
				return;

			try {
				if (session.Transaction.IsActive) {
					var fail = context.Exception != null;
					if (fail) {
						//если мы откатывается то в случае ошибки не нужно затирать оригинальную ошибку
						try {
							session.Transaction.Rollback();
						}
						catch(Exception e) {
							log.Error("Ошибка при откате транзакции", e);
						}
					}
					else {
						session.Flush();
						session.Transaction.Commit();
					}
				}
			}
			finally {
				session.Close();
			}
		}
	}
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
				AreaRegistration.RegisterAllAreas();
				RouteConfig.RegisterRoutes(RouteTable.Routes);
				var bundles = BundleTable.Bundles;
				bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
					"~/Scripts/jquery-{version}.js"));

				//bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
				//	"~/Scripts/jquery.validate*"));

				//// Use the development version of Modernizr to develop with and learn from. Then, when you're
				//// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
				//bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
				//	"~/Scripts/modernizr-*"));

				bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
					"~/Scripts/bootstrap.js",
					"~/Scripts/respond.js"));

				bundles.Add(new StyleBundle("~/Content/css").Include(
					"~/Content/bootstrap.css",
					"~/Content/site.css"));

			var nhibernate = new NHibernate();
			nhibernate.Init();
			GlobalFilters.Filters.Add(new SessionFilter(nhibernate.Factory));
		}
	}
}
