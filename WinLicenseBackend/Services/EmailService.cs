using System.Net.Mail;
using System.Net;

namespace WinLicenseBackend.Services
{
    public class EmailService
    {
        private AppSettings _settings;
        public EmailService(AppSettings appSettings) { 
            _settings = appSettings;
        }

        public void SendFileWithSmtpClient(
            byte[] fileBytes,
            string fileName,
            string contentType,
            string[] toEmails)
        {
            var mail = new MailMessage();
            mail.From = new MailAddress(_settings.EmailFrom, _settings.EmailTitle);
            foreach(string email in toEmails)
            {
                mail.To.Add(email);
            }           
            mail.Subject = "Your licenses are ready";
            mail.Body = "Please see the attached ZIP file containing your licenses.";

            // Create attachment from in-memory bytes
            var stream = new MemoryStream(fileBytes);
            var attachment = new Attachment(stream, fileName, contentType);
            mail.Attachments.Add(attachment);

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPass),
                EnableSsl = _settings.EnableSMTPssl,
            };
            client.Send(mail);
        }

    }
}
