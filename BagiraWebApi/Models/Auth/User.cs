namespace BagiraWebApi.Models.Auth
{
    public class User
    {
        public Guid Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public List<Session> Sessions { get; set; } = new();
    }
}
