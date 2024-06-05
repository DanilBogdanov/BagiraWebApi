namespace BagiraWebApi.Services.Messengers
{
    public interface IMessenger
    {
        Task SendMessageAsync(string title, string message);
    }
}
