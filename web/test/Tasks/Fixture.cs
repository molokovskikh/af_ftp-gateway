using System;
using web_app.Models;

namespace test.Tasks
{
	public class Fixture
	{
		public void Execute()
		{
			var init = new web_app.NHibernate();
			init.Init();
			using (var session = init.Factory.OpenSession())
			using (var trx = session.BeginTransaction()) {
				session.Save(new Admin {
					Login = "root",
					Name = "root"
				});
				trx.Commit();
			}
		}
	}
}