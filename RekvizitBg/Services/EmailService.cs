namespace RekvizitBg.Services
{
    using MailKit.Net.Smtp;
    using MimeKit;
    using Microsoft.Extensions.Options;

    public class EmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string messageBody)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            emailMessage.To.Add(new MailboxAddress("", recipientEmail));
            //emailMessage.To.Add(new MailboxAddress("information", "info@rekvizit.bg" ));         
            emailMessage.To.Add(new MailboxAddress("information", "rekvizit.bg@gmail.com"));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = messageBody };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, _smtpSettings.UseSsl);
                    await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                    await client.SendAsync(emailMessage);
                }
                catch (Exception ex)
                {
                    // Handle the exception (log it or display an error message)
                    throw;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }

    public class SmtpSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseSsl { get; set; }
    }

}
