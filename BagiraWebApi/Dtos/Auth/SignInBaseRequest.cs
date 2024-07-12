namespace BagiraWebApi.Dtos.Auth
{
    public class SignInBaseRequest
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
    }
}
