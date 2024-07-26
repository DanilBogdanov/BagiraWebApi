namespace BagiraWebApi.Dtos.Auth
{
    public class TokensDto
    {
        public required bool IsAnonymous { get; set; }
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required int ExpiresIn { get; set; }
    }
}
