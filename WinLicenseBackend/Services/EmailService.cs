using System.Net;
using WebApi.Helpers;
using System.Text;
using MimeKit.Cryptography;
using MimeKit;
using MailKit.Net.Smtp;
using System.IO;
using Microsoft.Extensions.Logging;

namespace WinLicenseBackend.Services
{
    public class EmailService
    {
        private readonly AppSettings _settings;
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public EmailService(AppSettings appSettings)
        {
            _settings = appSettings;
        }

        public async Task SendFileWithSmtpClient(
            byte[] fileBytes,
            string fileName,
            string[] productsList,
            string contentType,
            string toEmail,
            string customerName)
        {
            _logger.Info("Preparing email to {ToEmail} with file {FileName}", toEmail, fileName);

            try
            {
                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(_settings.EmailFrom));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = "Your licenses are ready";

                string body = FileReaderUtils.ReadFileContent("mail_template.html");
                body = body.Replace("%ProductsList%", PrepareProductsList(productsList));
                body = body.Replace("%CustomerName%", customerName);

                var htmlBody = new TextPart("html") { Text = body };

                var stream = new MemoryStream(fileBytes);
                var attachment = new MimePart("application", "zip")
                {
                    Content = new MimeContent(stream, ContentEncoding.Default),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = fileName
                };

                var multipart = new Multipart("mixed") { htmlBody, attachment };
                message.Body = multipart;
                message.Date = DateTimeOffset.UtcNow;

                _logger.Info("Signing message with DKIM for domain {Domain}", _settings.Domain);

                var signer = new DkimSigner("dkim/dkim-private-key.pem", _settings.Domain, "default")
                {
                    SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha256,
                    HeaderCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Relaxed,
                    BodyCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Relaxed
                };

                var headersToSign = new[] { "From", "To", "Subject", "Date" };
                signer.Sign(message, headersToSign);

                using var smtp = new SmtpClient();
                _logger.Info("Connecting to SMTP server {Host}:{Port}", _settings.SmtpHost, _settings.SmtpPort);

                await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, false);
                await smtp.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPass);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                _logger.Info("Email sent to {ToEmail} with attachment {FileName}", toEmail, fileName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send email to {ToEmail}", toEmail);
                throw;
            }
        }

        private string PrepareProductsList(string[] productsList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string product in productsList)
            {
                sb.Append("<li>");
                sb.Append(product);
                sb.Append("</li>");
            }
            return sb.ToString();
        }

    }
}
