using System.ComponentModel.DataAnnotations;

namespace web_app.Models
{
	public class Login
	{
		[Display(Name = "Имя пользователя"), Required]
		public string UserName { get; set; }

		[Display(Name = "Пароль"), Required]
		public string Password { get; set; }

		[Display(Name = "Запомнить меня")]
		public bool RememberMe { get; set; }
	}
}