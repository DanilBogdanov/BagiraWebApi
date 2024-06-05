using BagiraWebApi.Configs.Messenger;
using MailKit.Net.Smtp;
using MimeKit;

namespace BagiraWebApi.Services.Messengers
{
    public class EmailMessenger : IMessenger
    {
        private readonly string _userEmail;
        private readonly MessengerConfig _messengerConfig;
        private readonly ILogger<MessengerService> _logger;

        public EmailMessenger(string userEmail, MessengerConfig messengerConfig, ILogger<MessengerService> logger)
        {
            _userEmail = userEmail;
            _messengerConfig = messengerConfig;
            _logger = logger;
        }

        public async Task SendMessageAsync(string title, string message)
        {
            foreach (var emailConfig in _messengerConfig.Emails)
            {
                try
                {
                    await SendMessageAsync(emailConfig, title, message);

                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError("EmailMessenger \nfrom: {emailFrom} \nto: {emailTo} \nexception: {ex}"
                        , emailConfig.Email, _userEmail, ex.Message);
                }
            }

            _logger.LogCritical("Cannot send email");

            throw new Exception("Cannot send email");
        }

        private async Task SendMessageAsync(EmailConfig emailConfig, string title, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(_messengerConfig.FromTitle, emailConfig.Email));
            emailMessage.To.Add(new MailboxAddress("", _userEmail));
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
