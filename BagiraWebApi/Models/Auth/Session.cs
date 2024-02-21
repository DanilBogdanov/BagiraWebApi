namespace BagiraWebApi.Models.Auth
{
    public class Session
    {
        public Guid SessionId { get; set; }
        public Guid? UserId { get; set; }
        public User? User { get; set; }
        public required DateTime Expires { get; set; }
        public required Guid SessionToken { get; set; }
    }
}
