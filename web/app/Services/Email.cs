using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace web_app.Services
{
	public class Email
	{

		public static void SendEmail(string to, string subject, string body)
		{
			var sendMailToClientFlag = ConfigurationManager.AppSettings["SendMailToClientFlag"];
			if (string.IsNullOrEmpty(sendMailToClientFlag))
			{
				return;
			}
			var mail = new MailMessage();
			mail.To.Add(to);
			mail.From = new MailAddress(ConfigurationManager.AppSettings["MailSenderAddress"]);
			mail.Subject = subject;
			mail.Body = body;
			mail.IsBodyHtml = true;
			SmtpClient smtp = new SmtpClient();
			smtp.Host = ConfigurationManager.AppSettings["SmtpServer"];
			smtp.Port = 25;
			smtp.UseDefaultCredentials = false;
#if DEBUG
#else
					smtp.Send(mail);
#endif
		}
	}
}