namespace BagiraWebApi.Models.Auth
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Password { get; set; }
        public List<Session> Sessions { get; set; } = new();
    }
}
