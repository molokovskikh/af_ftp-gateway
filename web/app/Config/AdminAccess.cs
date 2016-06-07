using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using web_app.Interfaces;

namespace web_app.Config
{
	/// <summary>
	///запрет доступа обычным пользователям
	/// </summary>
	public class AdminActionAccess : Attribute
	{
	}
}