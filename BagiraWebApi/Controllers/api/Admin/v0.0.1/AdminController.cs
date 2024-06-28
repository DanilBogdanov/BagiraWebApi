using BagiraWebApi.Services.Messengers;
using Microsoft.AspNetCore.Mvc;
using TL;

namespace BagiraWebApi.Controllers.api.Admin.v0._0._1
{
    [Route("api/a/v0.0.1")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly WTelegramService _telegramService;
        private readonly MessengerService _messengerService;

        public AdminController(WTelegramService telegramService, MessengerService messengerService)
        {
            _telegramService = telegramService;
            _messengerService = messengerService;
        }

        [HttpGet("tg/status")]
        public ContentResult Status()
        {
            return _telegramService.ConfigNeeded switch
            {
                "connecting" => Content("<meta http-equiv=\"refresh\" content=\"1\">WTelegram is connecting...", "text/html"),
                null => Content($@"Connected as {_telegramService.Client.User.first_name}", "text/html; charset=utf-8"),
                _ => Content($@"Enter {_telegramService.ConfigNeeded}: <form action=""config""><input name=""value"" autofocus/></form>", "text/html"),
            };
        }

        [HttpGet("tg/config")]
        public async Task<ActionResult> Config(string value)
        {
            await _telegramService.DoLogin(value);
            return Redirect("status");
        }

        [HttpGet("tg/contacts")]
        public async Task<ContentResult> Contacts()
        {
            var clients = await _telegramService.Client.Contacts_GetContacts();

            var res = "";

            foreach (var client in clients.users)
            {
                res += $@"<br>{client.Value}/ {client.Value.phone}";
            }

            return Content(clients.users.Count + "<br>" + res, "text/html; charset=utf-8");
        }

        [HttpPost("sendMsg")]
        public async Task SendMsg(string phone, string msg)
        {
            await _messengerService.SendMessageAsync(MessageType.Telegram, phone , "Тест", msg);
        }
    }
}
