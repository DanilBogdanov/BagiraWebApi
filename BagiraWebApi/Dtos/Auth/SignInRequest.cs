namespace BagiraWebApi.Dtos.Auth
{
    public class SignInRequest : SignInBaseRequest
    {
        public required string Login { get; set; }
        public required string Code { get; set; }
    }
}
