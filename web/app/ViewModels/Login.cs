using System.ComponentModel.DataAnnotations;

namespace web_app.ViewModels
{
	public class Login
	{
		[Display(Name = "Имя пользователя")]
		[Required(ErrorMessage = "Введите имя пользователя")]
		[MinLength(3, ErrorMessage = "Минимальная длина имени пользователя - 3 символа")]
		[MaxLength(40, ErrorMessage = "Слишком длинное имя пользователя")]
		public string UserName { get; set; }

		[Display(Name = "Пароль")]
		[Required(ErrorMessage = "Введите пароль")]
		[MinLength(4, ErrorMessage = "Минимальная длина пароля - 4 символа")]
		public string Password { get; set; }

		[Display(Name = "Запомнить меня")]
		public bool RememberMe { get; set; }
	}
}