namespace BagiraWebApi.Dtos.Auth
{
    public class SignInRequestDTO
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public string? Login { get; set; }
        public string? Code { get; set; }
    }
}
