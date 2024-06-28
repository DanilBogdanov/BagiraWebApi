using BagiraWebApi.Configs.Messenger;
using Microsoft.Extensions.Options;
using TL;

namespace BagiraWebApi.Services.Messengers
{
    public sealed class WTelegramService : BackgroundService
    {
        public readonly WTelegram.Client Client;
        public string ConfigNeeded = "connecting";

        private readonly MessengerConfig _messengerConfig;

        public WTelegramService(IOptions<MessengerConfig> messengerConfig, ILogger<WTelegramService> logger)
        {
            _messengerConfig = messengerConfig.Value;
            WTelegram.Helpers.Log = (lvl, msg) => logger.Log((LogLevel)lvl, msg);
            Client = new WTelegram.Client(what => _messengerConfig.Telegram[what]);
        }

        public override void Dispose()
        {
            Client.Dispose();
            base.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConfigNeeded = await DoLogin(_messengerConfig.Telegram.PhoneNumber);
        }

        public async Task<string> DoLogin(string loginInfo)
        {
            return ConfigNeeded = await Client.Login(loginInfo);
        }
    }
}
