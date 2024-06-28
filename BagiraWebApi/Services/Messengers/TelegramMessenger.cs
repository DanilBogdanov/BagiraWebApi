using TL;
using WTelegram;

namespace BagiraWebApi.Services.Messengers
{
    public class TelegramMessenger : IMessenger
    {
        private readonly Client _client;

        public TelegramMessenger(Client client)
        {
            _client = client;
        }

        public async Task SendMessageAsync(string phone, string title, string message)
        {
            var contacts = await _client.Contacts_GetContacts();
            var user = contacts.users.FirstOrDefault(user => user.Value.phone == phone).Value;

            if (user == null)
            {
                var importedContacts = await _client.Contacts_ImportContacts(new[] { new InputPhoneContact { phone = $"+{phone}" } });

                if (importedContacts.imported.Length == 0) {
                    throw new WTException($"User with phone: {phone} doesn't have telegram account");
                }

                user = importedContacts.users[importedContacts.imported[0].user_id];
            }

            var text = $"<b>{title}</b>: {message}";
            var entities = _client.HtmlToEntities(ref text);
            var res = await _client.SendMessageAsync(user, text, entities: entities);
        }
    }
}
