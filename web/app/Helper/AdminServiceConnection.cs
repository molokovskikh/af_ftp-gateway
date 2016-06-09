using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace web_app.Helper
{
	/// <summary>
	/// Класс реализует обращение к интерфейсу администратора
	/// </summary>
	public class AdminServiceConnection
	{
		private readonly string mDomain = null;

		public AdminServiceConnection(string domain)
		{
			mDomain = domain;
		}

		public Task<string> ExecuteAction<T>(string request, string parametres)
		{
			if (string.IsNullOrEmpty(request) || String.IsNullOrEmpty(mDomain))
				return null;
			var mUrl = request;
			var httpClientHandler = new HttpClientHandler();
			if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["AdminUser"]))
				httpClientHandler.UseDefaultCredentials = true;
			else {
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
				var credentialCache = new CredentialCache();
				string login = ConfigurationManager.AppSettings["AdminUser"];
				string psw = ConfigurationManager.AppSettings["AdminPassword"];
				credentialCache.Add(new Uri(mUrl), "NTLM", new NetworkCredential(login, psw, mDomain));
				credentialCache.Add(new Uri(mUrl), "BASIC", new NetworkCredential(login, psw, mDomain));
				httpClientHandler.Credentials = credentialCache;
			}
			httpClientHandler.PreAuthenticate = true;
			var httpClient = new HttpClient(httpClientHandler) { Timeout = TimeSpan.FromMinutes(2) };
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
			return httpClient.GetStringAsync(mUrl + "?" + parametres);
		}
	}
}