namespace BagiraWebApi.Services.Messengers
{
    public interface IMessenger
    {
        Task SendMessageAsync(string contact, string title, string message);
    }
}
