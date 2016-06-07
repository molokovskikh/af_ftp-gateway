using System.Security.Cryptography.X509Certificates;

namespace web_app.Interfaces
{
	/// <summary>
	/// Интерфейс пользователя приложением
	/// </summary>
	public interface IWebOperator
	{
		/// <summary>
		/// идентификатор
		/// </summary>
		uint Id { get; set; }

		/// <summary>
		/// является ли пользователь админом (для упращенной системы авторизации)
		/// </summary>
		bool IsAdmin { get; }

		/// <summary>
		/// наименование
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// метод, возвращающий пользователя приложением
		/// </summary>
		/// <returns></returns>
		IWebOperator GetOperator();
	}
}