using System;
using System.Net;
using System.Net.Mail;

namespace BigCommercePinterestFeed
{
    /// <summary>
    /// Represents an instance of the EMail Notification class.
    /// </summary>
    public class EMailNotification
    {
        private string MailSMTPAddress;        
        private int MailPort;
        private string FromEMail;
        private string MailPassword;
        private string MailToAddress;

        /// <summary>
        /// Initializes a new instance of the Mail Notification class. 
        /// Establishes the necessary parameters to send email notifications.
        /// </summary>
        public EMailNotification(string MailSMTPAddress, int MailPort, string FromEMail, string MailPassword, string MailToAddress)
        {
            this.MailSMTPAddress = MailSMTPAddress;
            this.MailPort = MailPort;
            this.FromEMail = FromEMail;
            this.MailPassword = MailPassword;
            this.MailToAddress = MailToAddress;
        }
        /// <summary>
        /// Send Email notification using the specified subject and message.
        /// </summary>
        public void SendNotification(string storeName, string subject, string message)
        {
            try
            {
                //Set email provider credentials
                SmtpClient smtpClient = new SmtpClient(this.MailSMTPAddress, this.MailPort);
                NetworkCredential mailAuthentication = new NetworkCredential(this.FromEMail, this.MailPassword);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = mailAuthentication;

                MailAddress from = new MailAddress(this.FromEMail, storeName);
                MailAddress to = new MailAddress(this.MailToAddress);

                MailMessage MyMailMessage = new MailMessage(from, to);

                MyMailMessage.Subject = subject;
                MyMailMessage.Body = message;

                MyMailMessage.IsBodyHtml = false;

                smtpClient.Send(MyMailMessage);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
