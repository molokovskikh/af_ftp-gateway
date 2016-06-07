using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using web_app.Interfaces;
using web_app.Models;

namespace web_app.ViewModels
{
	/// <summary>
	/// Профиль стороннего пользователя
	/// </summary>
	public class OutsiderProfile : IDbModel<ISession, Outsider>
	{
		public OutsiderProfile()
		{
		}

		public OutsiderProfile(Outsider outsider)
		{
			Id = outsider.Id;
			Login = outsider.Login;
			UserName = outsider.Name;
			Enabled = outsider.Enabled;
		}

		public uint Id { get; set; }

		[Display(Name = "ФИО пользователя")]
		[Required(ErrorMessage = "Введите имя пользователя")]
		[MinLength(3, ErrorMessage = "Минимальная длина имени пользователя - 3 символа")]
		[MaxLength(40, ErrorMessage = "Слишком длинное имя пользователя")]
		public string UserName { get; set; }

		[Display(Name = "Логин пользователя")]
		[Required(ErrorMessage = "Введите имя пользователя")]
		[MinLength(3, ErrorMessage = "Минимальная длина имени пользователя - 3 символа")]
		[MaxLength(40, ErrorMessage = "Слишком длинное имя пользователя")]
		public string Login { get; set; }

		[Display(Name = "Пароль")]
		[MinLength(4, ErrorMessage = "Минимальная длина пароля - 4 символа")]
		[MaxLength(40, ErrorMessage = "Слишком длинный пароль")]
		public string Password { get; set; }

		[Display(Name = "Активен")]
		public bool Enabled { get; set; }

		public bool Register(Outsider outsider)
		{
			return false;
		}

		public bool Update(Outsider outsider)
		{
			return false;
		}

		public Outsider GetDbModel(ISession dbSession)
		{
			var outsider = new Outsider();
			var model = this;
			if (model.Id != 0) {
				outsider = dbSession.Query<Outsider>().FirstOrDefault(s => s.Id == model.Id);
				outsider.Login = model.Login;
				outsider.Enabled = model.Enabled;
				outsider.Name = model.UserName;
			}
			else {
				outsider.Login = model.Login;
				outsider.Enabled = model.Enabled;
				outsider.Name = model.UserName;
			}
			return outsider;
		}
	}
}