using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;
using web_app.Models;

namespace test.Functional.User
{
	class ClientFixture : BaseFixture
	{
		[Test]
		public void AddClientAsAdmin()
		{
			var client = DbSession.Query<Client>().OrderByDescending(s => s.Id).FirstOrDefault();
			var user = DbSession.Query<web_app.Models.User>().OrderByDescending(s => s.Id).FirstOrDefault();
			user.ClientId = client.Id;
			user.UseFtpGateway = true;
			DbSession.Save(user);
			DbSession.Transaction.Commit();
			TryUpdateTransaction();
			var blockNameNew = ".container.body-content ";
			LoginAsAdmin();
			//
			WaitForVisibleCss("input[name='search']");
			//search
			var inputObj = browser.FindElementByCssSelector(blockNameNew + "input[name='search']");
			inputObj.Clear();
			inputObj.SendKeys(client.Name);
			browser.FindElementByCssSelector(blockNameNew + "button[type='submit']").Click();
			WaitForVisibleCss("table");
			browser.FindElementByCssSelector(blockNameNew + "table a").Click();
			AssertText(user.Name);
			browser.FindElementByCssSelector(blockNameNew + ".panel .btn.btn-success").Click();
			AssertText("Ваш логин: newLogin, пароль: newPass");
			AssertText("Инструкция эксплуатации FTP-сервиса");
			browser.FindElementByCssSelector("a[id='logoutLink']").Click();
			AssertText("Введите учетные данные");
		}

		[Test]
		public void AddClientAsOutsider()
		{
			var client = DbSession.Query<Client>().OrderByDescending(s => s.Id).FirstOrDefault();
			var user = DbSession.Query<web_app.Models.User>().OrderByDescending(s => s.Id).FirstOrDefault();
			user.ClientId = client.Id;
			user.UseFtpGateway = true;
			DbSession.Save(user);
			DbSession.Transaction.Commit();
			TryUpdateTransaction();
			var blockNameNew = ".container.body-content ";
			LoginAsOutsider();
			//
			WaitForVisibleCss("input[name='search']");
			//search
			var inputObj = browser.FindElementByCssSelector(blockNameNew + "input[name='search']");
			inputObj.Clear();
			inputObj.SendKeys(client.Name);
			browser.FindElementByCssSelector(blockNameNew + "button[type='submit']").Click();
			WaitForVisibleCss("table");
			browser.FindElementByCssSelector(blockNameNew + "table a").Click();
			AssertText(user.Name);
			browser.FindElementByCssSelector(blockNameNew + ".panel .btn.btn-success").Click();
			AssertText("Ваш логин: newLogin, пароль: newPass");
			AssertText("Инструкция эксплуатации FTP-сервиса");
			browser.FindElementByCssSelector("a[id='logoutLink']").Click();
			AssertText("Введите учетные данные");
		}
	}
}