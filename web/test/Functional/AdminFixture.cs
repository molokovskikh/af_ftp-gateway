using System;
using System.Configuration;
using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;
using test.DataFactory;
using web_app.Models;

namespace test.Functional
{
	class AdminFixture : BaseFixture
	{
		[Test]
		public void LoginSuccess()
		{
			var item = DbSession.Query<Admin>().OrderByDescending(s => s.Id).FirstOrDefault();
			var blockNameNew = "#loginForm ";
			string login = item.Login;
			string password = ConfigurationManager.AppSettings["DefaultOperatorPassword"];
			//
			Open("Account/Login");
			//login
			var inputObj = browser.FindElementByCssSelector(blockNameNew + "input[id='UserName']");
			inputObj.Clear();
			inputObj.SendKeys(login);
			//password
			inputObj = browser.FindElementByCssSelector(blockNameNew + "input[id='Password']");
			inputObj.Clear();
			inputObj.SendKeys(password);
			browser.FindElementByCssSelector(blockNameNew + "input[type='submit']").Click();
			AssertText("Пользователь " + login);
			browser.FindElementByCssSelector("a[id='logoutLink']").Click();
			AssertText("Введите учетные данные");
		}

		[Test]
		public void LoginError()
		{
			var item = DbSession.Query<Admin>().OrderByDescending(s => s.Id).FirstOrDefault();
			var blockNameNew = "#loginForm ";
			string login = item.Login;
			string password = "error";
			//
			Open("Account/Login");
			//login
			if (browser.FindElementsByCssSelector(blockNameNew + "input[id='UserName']").Count == 0) {
				throw new Exception(browser.FindElementByCssSelector("body").Text);
			}
			var inputObj = browser.FindElementByCssSelector(blockNameNew + "input[id='UserName']");
			inputObj.Clear();
			inputObj.SendKeys(login);
			//password
			inputObj = browser.FindElementByCssSelector(blockNameNew + "input[id='Password']");
			inputObj.Clear();
			inputObj.SendKeys(password);
			browser.FindElementByCssSelector(blockNameNew + "input[type='submit']").Click();
			AssertText("Учетные данные введены неверно!");
			//
			login = "12";
			password = "1";
			//login
			inputObj = browser.FindElementByCssSelector(blockNameNew + "input[id='UserName']");
			inputObj.Clear();
			inputObj.SendKeys(login);
			//password
			inputObj = browser.FindElementByCssSelector(blockNameNew + "input[id='Password']");
			inputObj.Clear();
			inputObj.SendKeys(password);
			browser.FindElementByCssSelector(blockNameNew + "input[type='submit']").Click();
			AssertText("Минимальная длина имени пользователя");
			AssertText("Минимальная длина пароля");
		}

		[Test]
		public void RegisterUsers()
		{
			var newOutsider = CreateData.CreateOutsider(DbSession, dontSave: true);
			string password = ConfigurationManager.AppSettings["DefaultOperatorPassword"];
			var outsider = DbSession.Query<web_app.Models.Outsider>().OrderByDescending(s => s.Id).FirstOrDefault();
			var blockNameNew = ".container.body-content ";
			LoginAsAdmin();
			browser.FindElementByCssSelector("a[id='LinkUserList']").Click();
			WaitForVisibleCss("table");
			AssertText(outsider.Name);
			browser.FindElementByCssSelector(".panel .btn.btn-success").Click();
			WaitForVisibleCss("[id='UserName']");
			//UserName
			var inputObj = browser.FindElementByCssSelector(blockNameNew + "input[id='UserName']");
			inputObj.Clear();
			inputObj.SendKeys(newOutsider.Name);
			//Login
			inputObj = browser.FindElementByCssSelector(blockNameNew + "input[id='Login']");
			inputObj.Clear();
			inputObj.SendKeys(newOutsider.Login);
			//Password
			inputObj = browser.FindElementByCssSelector(blockNameNew + "input[id='Password']");
			inputObj.Clear();
			inputObj.SendKeys(password);
			browser.FindElementByCssSelector(blockNameNew + "input[name='Enabled']").Click();
			browser.FindElementByCssSelector(blockNameNew + "input[type='submit']").Click();
			WaitForVisibleCss("table");
			AssertText(newOutsider.Name);
			newOutsider = DbSession.Query<web_app.Models.Outsider>().FirstOrDefault(s => s.Login == newOutsider.Login);
			browser.FindElementByCssSelector(blockNameNew + @"a[href*='/" + newOutsider.Id + "']").Click();
			WaitForVisibleCss("[id='UserName']");
			//UserName
			inputObj = browser.FindElementByCssSelector(blockNameNew + "input[id='UserName']");
			inputObj.Clear();
			inputObj.SendKeys("new" + newOutsider.Name);
			//Login
			inputObj = browser.FindElementByCssSelector(blockNameNew + "input[id='Login']");
			inputObj.Clear();
			inputObj.SendKeys("new" + newOutsider.Login);
			browser.FindElementByCssSelector(blockNameNew + "input[type='submit']").Click();
			DbSession.Refresh(newOutsider);
			WaitForVisibleCss("table");
			AssertText(newOutsider.Name);
		}
	}
}