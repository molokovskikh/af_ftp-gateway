using System;
using System.Web.Mvc;
using log4net;
using NHibernate;

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
			var session = factory.OpenSession();
			try {
				session.BeginTransaction();
				context.HttpContext.Items[typeof(ISession)] = session;
			}
			catch {
				session.Close();
				throw;
			}
		}

		public override void OnActionExecuted(ActionExecutedContext context)
		{
			var session = (ISession)context.HttpContext.Items[typeof(ISession)];
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
}