using System.ComponentModel.DataAnnotations;

namespace BagiraWebApi.Configs.Messenger
{
    public class MessengerConfig
    {
        [Required]
        public required string FromTitle { get; init; }
        [Required]
        public required EmailConfig[] Emails { get; init; }
        [Required]
        public required TelegramConfig Telegram { get; init; }
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

    public class TelegramConfig
    {
        [Required]
        public required string ApiId { get; init; }
        [Required]
        public required string ApiHash { get; init; }
        [Required]
        public required string PhoneNumber { get; init; }
        [Required]
        public required string SessionPathname { get; init; }

        public string? this[string propName]
        {
            get
            {
                return propName switch
                {
                    "api_id" => ApiId,
                    "api_hash" => ApiHash,
                    "phone_number" => PhoneNumber,
                    "session_pathname" => SessionPathname,
                    _ => null,
                };
            }
        }

    }
}
