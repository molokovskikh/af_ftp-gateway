﻿using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using log4net;
using log4net.Config;
using web_app.Config;

namespace web_app
{
	public class MvcApplication : System.Web.HttpApplication
	{
		private static ILog _log = LogManager.GetLogger(typeof(MvcApplication));

		protected void Application_Start()
		{
			XmlConfigurator.Configure();
			GlobalContext.Properties["Version"] = typeof(MvcApplication).Assembly.GetName().Version;
			AreaRegistration.RegisterAllAreas();
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			var bundles = BundleTable.Bundles;
			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
				"~/Scripts/jquery-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/script").Include(
				"~/Scripts/bootstrap.js",
				"~/Scripts/respond.js",
				"~/Scripts/jquery.cookie.js",
				"~/Scripts/default.js"));

			bundles.Add(new StyleBundle("~/Content/css").Include(
				"~/Content/bootstrap.css",
				"~/Content/default.css"));

			var nhibernate = new NHibernate();
			nhibernate.Init();
			GlobalFilters.Filters.Add(new SessionFilter(nhibernate.Factory), 0);
			GlobalFilters.Filters.Add(new CurrentWebOperatorFilter(), 1);
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			var ex = Server.GetLastError();
			_log.Error("Ошибка приложения", ex);
			HttpContext.Current.ClearError();
			HttpContext.Current.Response.Redirect("~/Pure/Error.html");
		}
	}
}