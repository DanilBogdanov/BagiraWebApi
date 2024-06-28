using BagiraWebApi.Configs.Messenger;
using BagiraWebApi.Models.Auth;
using Microsoft.Extensions.Options;

namespace BagiraWebApi.Services.Messengers
{
    public class MessengerService
    {
        private readonly MessengerConfig _messengerConfig;
        private readonly ILogger<MessengerService> _logger;
        private readonly WTelegram.Client _telegramClient;

        public MessengerService(IOptions<MessengerConfig> messengerConfig, WTelegramService wTelegramService, ILogger<MessengerService> logger)
        {
            _messengerConfig = messengerConfig.Value;
            _telegramClient = wTelegramService.Client;
            _logger = logger;
        }

        public async Task SendMessageAsync(MessageType messageType, string contact, string title, string message)
        {
            IMessenger messenger = GetMessenger(messageType);

            await messenger.SendMessageAsync(contact, title, message);
        }

        private IMessenger GetMessenger(MessageType messageType)
        {
            return messageType switch
            {
                MessageType.Email => new EmailMessenger(_messengerConfig, _logger),
                MessageType.Telegram => new TelegramMessenger(_telegramClient),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
