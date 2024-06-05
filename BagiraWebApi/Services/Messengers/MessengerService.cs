using BagiraWebApi.Configs.Messenger;
using BagiraWebApi.Models.Auth;
using Microsoft.Extensions.Options;

namespace BagiraWebApi.Services.Messengers
{
    public class MessengerService
    {
        private readonly MessengerConfig _messengerConfig;
        private readonly ILogger<MessengerService> _logger;

        public MessengerService(IOptions<MessengerConfig> messengerConfig, ILogger<MessengerService> logger)
        {
            _messengerConfig = messengerConfig.Value;
            _logger = logger;
        }

        public async Task SendMessageAsync(User user, string title, string message)
        {
            IMessenger messenger = GetMessenger(user);

            await messenger.SendMessageAsync(title, message);
        }

        private IMessenger GetMessenger(User user)
        {
            if (user.Email != null)
            {
                return new EmailMessenger(user.Email, _messengerConfig, _logger);
            }

            throw new Exception("User must have email or phone");
        }
    }
}
