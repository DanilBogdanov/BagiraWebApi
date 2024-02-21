namespace BagiraWebApi.Services.Auth.DTO
{
    public class SignUpUserForm
    {
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
