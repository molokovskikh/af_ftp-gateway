using System;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using web_app.Models;

namespace test.DataFactory
{
	class CreateData
	{
		public static void FillTables(ISession dbSession)
		{
			CreateClient(dbSession);
			CreateUser(dbSession);
			CreateAdmin(dbSession);
			CreateOutsider(dbSession);
			dbSession.Flush();
			dbSession.Transaction.Commit();
		}

		public static void CleanTables()
		{
			//	"TRUNCATE TABLE "
		}

		public static User CreateUser(ISession dbSession, User user = null, bool dontSave = false)
		{
			if (user == null) {
				user = new User {
					ClientId = dbSession.Query<Client>().OrderByDescending(s => s.Id).FirstOrDefault().Id,
					Login = "login_" + new Random().Next(10000, 99999),
					Name = "name_" + new Random().Next(10000, 99999),
					Enabled = true
				};
			}
			user.Login = "login_" + new Random().Next(10000, 99999);
			while (dbSession.Query<User>().Any(s => s.Login == user.Login)) {
				user.Login = "login_" + new Random().Next(10000, 99999);
			}
			if (!dontSave)
				dbSession.Save(user);
			return user;
		}

		public static Client CreateClient(ISession dbSession, Client client = null, bool dontSave = false)
		{
			if (client == null) {
				client = new Client {
					Name = "name_" + new Random().Next(100, 999),
					FullName = "fullName_" + new Random().Next(100, 999),
					FtpIntegration = true
				};
			}
			if (!dontSave)
				dbSession.Save(client);
			return client;
		}

		public static Outsider CreateOutsider(ISession dbSession, Outsider outsider = null, bool dontSave = false)
		{
			if (outsider == null) {
				outsider = new Outsider {
					Login = "login_" + new Random().Next(10000, 99999),
					Name = "name_" + new Random().Next(10000, 99999),
					Enabled = true
				};
			}
			outsider.Login = "login_" + new Random().Next(10000, 99999);
			while (dbSession.Query<Outsider>().Any(s => s.Login == outsider.Login)) {
				outsider.Login = "login_" + new Random().Next(10000, 99999);
			}
			if (!dontSave)
				dbSession.Save(outsider);
			return outsider;
		}

		public static Admin CreateAdmin(ISession dbSession, Admin admin = null, bool dontSave = false)
		{
			if (admin == null) {
				admin = new Admin {
					Login = "login_" + new Random().Next(10000, 99999),
					Name = "name_" + new Random().Next(10000, 99999)
				};
			}
			admin.Login = "login_" + new Random().Next(10000, 99999);
			while (dbSession.Query<Admin>().Any(s => s.Login == admin.Login)) {
				admin.Login = "login_" + new Random().Next(10000, 99999);
			}
			if (!dontSave)
				dbSession.Save(admin);
			return admin;
		}
	}
}