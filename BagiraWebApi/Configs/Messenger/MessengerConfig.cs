using System.ComponentModel.DataAnnotations;

namespace BagiraWebApi.Configs.Messenger
{
    public class MessengerConfig
    {
        [Required]
        public required EmailConfig[] Emails { get; init; }
    }

    public class EmailConfig
    {
        [Required]
        public required string Email { get; init; }
        [Required]
        public required string Password { get; init; }
        [Required]
        public required string Url { get; init; }
        [Required]
        public required int Port { get; init; }
    }
}
