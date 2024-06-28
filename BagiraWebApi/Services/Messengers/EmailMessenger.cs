using BagiraWebApi.Configs.Messenger;
using MailKit.Net.Smtp;
using MimeKit;

namespace BagiraWebApi.Services.Messengers
{
    public class EmailMessenger : IMessenger
    {
        private readonly MessengerConfig _messengerConfig;
        private readonly ILogger<MessengerService> _logger;

        public EmailMessenger(MessengerConfig messengerConfig, ILogger<MessengerService> logger)
        {
            _messengerConfig = messengerConfig;
            _logger = logger;
        }

        public async Task SendMessageAsync(string emailTo, string title, string message)
        {
            foreach (var emailConfig in _messengerConfig.Emails)
            {
                try
                {
                    await SendMessageAsync(emailConfig, emailTo, title, message);

                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError("EmailMessenger \nfrom: {emailFrom} \nto: {emailTo} \nexception: {ex}"
                        , emailConfig.Email, emailTo, ex.Message);
                }
            }

            var errMsg = "Cannot send email";

            _logger.LogCritical(errMsg);

            throw new Exception(errMsg);
        }

        private async Task SendMessageAsync(EmailConfig emailConfig, string emailTo, string title, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(_messengerConfig.FromTitle, emailConfig.Email));
            emailMessage.To.Add(new MailboxAddress("", emailTo));
            emailMessage.Subject = title;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(emailConfig.Url, emailConfig.Port, true);
            await client.AuthenticateAsync(emailConfig.Email, emailConfig.Password);
            await client.SendAsync(emailMessage);

            await client.DisconnectAsync(true);
        }
    }
}
