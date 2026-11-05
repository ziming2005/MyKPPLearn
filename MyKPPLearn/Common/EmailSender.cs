using System.Net.Mail;
using System.Text;

namespace MyKPPLearn.Common
{
    /// <summary>
    /// Sends email through the SMTP server in Web.config (system.net/mailSettings). The sender address,
    /// server, port and login all come from there, so nothing about the mail account is in code.
    /// </summary>
    public static class EmailSender
    {
        public static void Send(string toAddress, string subject, string plainTextBody)
        {
            using (MailMessage message = new MailMessage())
            {
                message.To.Add(toAddress);
                message.Subject = subject;
                message.Body = plainTextBody;
                message.BodyEncoding = Encoding.UTF8;
                message.SubjectEncoding = Encoding.UTF8;
                message.IsBodyHtml = false;

                using (SmtpClient client = new SmtpClient())
                {
                    client.Send(message);
                }
            }
        }
    }
}
