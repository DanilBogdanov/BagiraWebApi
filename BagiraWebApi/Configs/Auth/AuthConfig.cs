using System.ComponentModel.DataAnnotations;

namespace BagiraWebApi.Configs.Auth
{
    public class AuthConfig
    {
        [Required]
        public required string Issuer { get; init; }
        [Required]
        public required string Audience { get; init; }
        [Required]
        public required string AccessKey { get; init; }
        [Required]
        public required string RefreshKey { get; init; }
        [Required]
        public required int TokenValidityInMinutes { get; init; }
        [Required]
        public required int RefreshTokenValidityInDays { get; init; }
        [Required]
        public required Client[] Clients { get; init; }
    }

    public class Client
    {
        [Required]
        public required string Id { get; init; }
        [Required]
        public required string Secret { get; init; }
    }
}
