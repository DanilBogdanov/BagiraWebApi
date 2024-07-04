namespace BagiraWebApi.Dtos.Auth
{
    public abstract class SignInAbstractDTO
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
    }
}
