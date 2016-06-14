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
			AddClient(true);
		}

		[Test]
		public void AddClientAsOutsider()
		{
			AddClient(false);
		}

		public void AddClient(bool asAdmin)
		{
			TryUpdateTransaction();
			var client = DbSession.Query<Client>().OrderByDescending(s => s.Id).FirstOrDefault();
			client.FtpIntegration = false;
			DbSession.Save(client);
			DbSession.Transaction.Commit();
			TryUpdateTransaction();
			var blockNameNew = ".container.body-content ";
			if (asAdmin) {
				LoginAsAdmin();
			}
			else {
				LoginAsOutsider();
			}
			WaitForVisibleCss("input[name='search']");
			//search клиент без ftpintegration отсутствует
			var inputObj = browser.FindElementByCssSelector(blockNameNew + "input[name='search']");
			inputObj.Clear();
			inputObj.SendKeys(client.Name);
			browser.FindElementByCssSelector(blockNameNew + "button[type='submit']").Click();
			WaitForVisibleCss("table");
			Assert.That(browser.FindElementsByCssSelector(blockNameNew + "table a").Count, Is.EqualTo(0));
			TryUpdateTransaction();
			client.FtpIntegration = true;
			DbSession.Save(client);
			DbSession.Transaction.Commit();
			TryUpdateTransaction();
			//search
			inputObj = browser.FindElementByCssSelector(blockNameNew + "input[name='search']");
			inputObj.Clear();
			inputObj.SendKeys(client.Name);
			browser.FindElementByCssSelector(blockNameNew + "button[type='submit']").Click();
			WaitForVisibleCss("table");
			browser.FindElementByCssSelector(blockNameNew + "table a").Click();
			browser.FindElementByCssSelector(blockNameNew + ".panel .btn.btn-success").Click();
			AssertText("Ваш логин: newLogin, пароль: newPass");
			AssertText("Инструкция эксплуатации FTP-сервиса");
			var user = DbSession.Query<web_app.Models.User>().FirstOrDefault(s => s.ClientId == client.Id);
			AssertText(user.Login);
			browser.FindElementByCssSelector("a[id='logoutLink']").Click();
			AssertText("Введите учетные данные");
		}
	}
}