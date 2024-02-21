namespace BagiraWebApi.Services.Auth.DTO
{
    public class SignInForm
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
