using System.ComponentModel.DataAnnotations;

namespace BagiraWebApi.Services.Exchanges
{
    public class Connection1CConfig
    {
        [Required]
        public required string DevHost { get; init; }
        [Required]
        public required string Host { get; init; }
        [Required]
        public required string Login { get; init; }
        [Required]
        public required string Password { get; init; }        
    }
}
