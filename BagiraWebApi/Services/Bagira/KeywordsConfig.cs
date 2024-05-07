using System.ComponentModel.DataAnnotations;

namespace BagiraWebApi.Services.Bagira
{
    public class KeywordsConfig
    {
        [Required]
        public required KeyValue[] Animal { get; init; }
    }

    public class KeyValue
    {
        [Required]
        public required string Key { get; init; }
        [Required]
        public required string Value { get; init; }
    }
}
