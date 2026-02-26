namespace RekvizitBg.Services
{
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using MimeKit;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Logging;

    public class EmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailService> _logger;

        // Recipient for all contact form submissions
        private const string NotificationRecipient = "kratos4o210@gmail.com";

        public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
        {
            _smtpSettings = smtpSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Sends a contact form notification email.
        /// Email is sent FROM the configured sender TO the notification recipient.
        /// </summary>
        public async Task<bool> SendContactFormAsync(string senderName, string senderEmail, string senderPhone, string subject, string messageBody)
        {
            try
            {
                var emailMessage = new MimeMessage();

                // FROM: the configured SMTP sender (Zoho)  
                emailMessage.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));

                // TO: the notification recipient
                emailMessage.To.Add(new MailboxAddress("Реквизит БГ", NotificationRecipient));

                // REPLY-TO: the form submitter's email (so you can reply directly)
                emailMessage.ReplyTo.Add(new MailboxAddress(senderName, senderEmail));

                emailMessage.Subject = $"[Реквизит БГ] {subject}";

                // Build a professional HTML email body
                var htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background: #1a1a1a; color: #e0e0e0; border-radius: 8px; overflow: hidden;'>
                        <div style='background: linear-gradient(135deg, #c9a84c, #d4a33a); padding: 20px 30px;'>
                            <h1 style='margin: 0; color: #0f0f0f; font-size: 24px;'>📧 Ново запитване</h1>
                        </div>
                        <div style='padding: 30px;'>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr><td style='padding: 8px 0; color: #c9a84c; font-weight: bold;'>Име:</td><td style='padding: 8px 0;'>{senderName}</td></tr>
                                <tr><td style='padding: 8px 0; color: #c9a84c; font-weight: bold;'>Имейл:</td><td style='padding: 8px 0;'><a href='mailto:{senderEmail}' style='color: #c9a84c;'>{senderEmail}</a></td></tr>
                                <tr><td style='padding: 8px 0; color: #c9a84c; font-weight: bold;'>Телефон:</td><td style='padding: 8px 0;'>{senderPhone}</td></tr>
                            </table>
                            <hr style='border: none; border-top: 1px solid #333; margin: 20px 0;' />
                            <h3 style='color: #c9a84c; margin-bottom: 10px;'>Съобщение:</h3>
                            <div style='background: #0f0f0f; padding: 15px; border-radius: 6px; border-left: 3px solid #c9a84c; line-height: 1.6;'>
                                {messageBody}
                            </div>
                        </div>
                        <div style='background: #0f0f0f; padding: 15px 30px; text-align: center; font-size: 12px; color: #666;'>
                            Изпратено от контактната форма на rekvizit.bg
                        </div>
                    </div>";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody,
                    TextBody = $"Име: {senderName}\nИмейл: {senderEmail}\nТелефон: {senderPhone}\n\nСъобщение:\n{messageBody}"
                };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                // Timeouts for reliability
                client.Timeout = 15000; // 15 seconds

                if (_smtpSettings.UseSsl)
                {
                    await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.SslOnConnect);
                }
                else
                {
                    await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
                }

                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Recipient} from {Sender}", NotificationRecipient, senderEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email from {SenderEmail}", senderEmail);
                return false;
            }
        }
    }

    public class SmtpSettings
    {
        public string Server { get; set; } = string.Empty;
        public int Port { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseSsl { get; set; }
    }
}
