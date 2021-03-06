﻿using System;
using System.Configuration;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;
using test.DataFactory;

namespace test.Functional
{
	class OutsiderFixture : BaseFixture
	{
		[Test]
		public void LoginSuccess()
		{
			session.BeginTransaction();
			var item = session.Query<web_app.Models.Outsider>().OrderByDescending(s => s.Id).FirstOrDefault();
			item.Enabled = true;
			session.Save(item);
			session.Transaction.Commit();
			var blockNameNew = "#loginForm ";
			Open("Account/Login");
			//login
			if (browser.FindElementsByCssSelector(blockNameNew + "input[id='UserName']").Count == 0) {
				throw new Exception(browser.FindElementByCssSelector("body").Text);
			}
			var inputObj = browser.FindElementByCssSelector(blockNameNew + "input[id='UserName']");
			inputObj.Clear();
			inputObj.SendKeys(item.Login);
			//password
			inputObj = browser.FindElementByCssSelector(blockNameNew + "input[id='Password']");
			inputObj.Clear();
			inputObj.SendKeys(ConfigurationManager.AppSettings["DefaultOperatorPassword"]);
			browser.FindElementByCssSelector(blockNameNew + "input[type='submit']").Click();
			AssertText("Пользователь " + item.Login);
			browser.FindElementByCssSelector("a[id='logoutLink']").Click();
			AssertText("Введите учетные данные");
		}

		[Test]
		public void LoginError()
		{
			session.BeginTransaction();
			CreateData.CreateOutsider(session, new web_app.Models.Outsider() {
				Login = "login_" + new Random().Next(100, 999),
				Name = "name_" + new Random().Next(100, 999),
				Enabled = false
			});
			var item = session.Query<web_app.Models.Outsider>().OrderByDescending(s => s.Id).FirstOrDefault();
			var blockNameNew = "#loginForm ";
			string login = item.Login;
			string password = ConfigurationManager.AppSettings["DefaultOperatorPassword"];
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
			password = "error";
			//login
			inputObj = browser.FindElementByCssSelector(blockNameNew + "input[id='UserName']");
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
	}
}