using System;
using System.Diagnostics;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace DTO_PremierDucts.Utils
{
	public class EmailUtils
	{
		public static void SendEmail(string filename, string subject, params string[] emails)
		{
			try
			{
				MailMessage mail = new MailMessage();
				SmtpClient SmtpServer = new SmtpClient("smtp.office365.com");
				mail.From = new MailAddress("noreply@premierducts.com.au");

				foreach (string email in emails)
				{
					mail.To.Add(email);
				}
				mail.Subject = subject;
				mail.Body = "mail with attachment";
				System.Net.Mail.Attachment attachment;
				attachment = new System.Net.Mail.Attachment(filename + ".xlsx");
				mail.Attachments.Add(attachment);
				SmtpServer.Port = 587;
				SmtpServer.Credentials = new System.Net.NetworkCredential("noreply@premierducts.com.au", "Cricket!wisdom211$");
				SmtpServer.EnableSsl = true;
				SmtpServer.Send(mail);
				Debug.Write("Email sent");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				throw ex;
			}
		}
	}
}

