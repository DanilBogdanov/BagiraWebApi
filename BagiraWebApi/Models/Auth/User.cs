namespace BagiraWebApi.Models.Auth
{
    public class User
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public required string VerificationCode { get; set; }
        public byte AccessFailedCount { get; set; }
        public bool Confirmed { get; set; }
        public List<Session> Sessions { get; set; } = new();
    }
}
