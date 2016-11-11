using System.Configuration;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;
using test.DataFactory;
using Test.Support.Selenium;
using web_app.Interfaces;
using web_app.Models;

namespace test
{
	public class BaseFixture : SeleniumFixture
	{
		protected IWebOperator CurrenOperator { get; set; }

		[SetUp]
		public void DefaultOnSetup()
		{
			CreateData.FillTables(session);
		}

		[TearDown]
		public void DefaultOnTearDown()
		{
			if (CurrenOperator != null && browser.FindElementsById("logoutLink").Count > 0) {
				CurrenOperator = null;
				browser.FindElementByCssSelector("a[id='logoutLink']").Click();
			}
			CloseAllTabsButOne();
		}

		public Admin LoginAsAdmin()
		{
			var item = session.Query<Admin>().OrderByDescending(s => s.Id).FirstOrDefault();
			var blockNameNew = "#loginForm ";
			string login = item.Login;
			string password = ConfigurationManager.AppSettings["DefaultOperatorPassword"];
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
			CurrenOperator = item;
			return item;
		}

		public void CloseAllTabsButOne()
		{
			var allTabsToClose = GlobalDriver.WindowHandles.ToList();

			if (allTabsToClose.Count > 1)
				for (int i = 1; i < allTabsToClose.Count; i++) {
					if (GlobalDriver.CurrentWindowHandle != allTabsToClose[i]) {
						GlobalDriver.SwitchTo().Window(allTabsToClose[i]);
						GlobalDriver.Close();
					}
				}
		}

		public Outsider LoginAsOutsider()
		{
			var item = session.Query<Outsider>().OrderByDescending(s => s.Id).FirstOrDefault();
			item.Enabled = true;
			session.Save(item);

			session.Transaction.Commit();

			var blockNameNew = "#loginForm ";
			string login = item.Login;
			string password = ConfigurationManager.AppSettings["DefaultOperatorPassword"];

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
			CurrenOperator = item;
			return item;
		}
	}
}