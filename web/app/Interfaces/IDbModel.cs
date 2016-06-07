using System.Security.Cryptography.X509Certificates;

namespace web_app.Interfaces
{
	/// <summary>
	/// Обновление модели данных по моделе представления
	/// </summary>
	/// <typeparam name="D">Контекст БД (сессия)</typeparam>
	/// <typeparam name="T">Тип модели данных</typeparam>
	public interface IDbModel<in D, out T>
	{
		T GetDbModel(D dbSession);
	}
}