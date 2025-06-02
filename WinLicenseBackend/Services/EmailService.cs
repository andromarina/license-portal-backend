
using System.Net;
using WebApi.Helpers;
using System.Text;
using MimeKit.Cryptography;
using MimeKit;
using MailKit.Net.Smtp;
using System.IO;

namespace WinLicenseBackend.Services
{
    public class EmailService
    {
        private AppSettings _settings;
        public EmailService(AppSettings appSettings) { 
            _settings = appSettings;
        }

        public async Task SendFileWithSmtpClient(byte[] fileBytes,
            string fileName,
            string[] productsList,
            string contentType,
            string toEmail,
            string customerName)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_settings.EmailFrom));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Your licenses are ready";
            string body = FileReaderUtils.ReadFileContent("mail_template.html");
            body = body.Replace("%ProductsList%", PrepareProductsList(productsList));
            body = body.Replace("%CustomerName%", customerName);
            var htmlBody = new TextPart("html")
            {
                Text = body,
            };

            var stream = new MemoryStream(fileBytes);
            // Load the attachment
            var attachment = new MimePart("application", "zip") // adjust MIME type as needed
            {
                Content = new MimeContent(stream, ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = fileName
            };

            // Combine into multipart
            var multipart = new Multipart("mixed");
            multipart.Add(htmlBody);
            multipart.Add(attachment);

            // Set the message body
            message.Body = multipart;
            message.Date = DateTimeOffset.UtcNow;
           
            var signer = new DkimSigner("dkim/dkim-private-key.pem", _settings.Domain, "default")
            {
                SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha256,
                HeaderCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Relaxed,
                BodyCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Relaxed
            };

            var headersToSign = new[] { "From", "To", "Subject", "Date" };
            signer.Sign(message, headersToSign); 

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, false);
            await smtp.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPass);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            Console.WriteLine("DKIM signed message sent.");
        }


        private string PrepareProductsList(string[] productsList)
        {
            StringBuilder sb = new StringBuilder();
            foreach(string product in productsList)
            {
                sb.Append("<li>");
                sb.Append(product);
                sb.Append("</li>");
            }
            return sb.ToString();
        }

    }
}
