using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace web_app
{
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
