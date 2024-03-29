using System.ComponentModel.DataAnnotations;

namespace BagiraWebApi.Services.Bagira
{
    public class BagiraConfig
    {
        [Required]
        public required int DefaultStorage { get; init; }
        [Required]
        public required int DefaultPriceType { get; init; }
        [Required]
        public required Properties Properties { get; init; }
    }

    public class Properties
    {
        [Required]
        public required Ids Ids { get; init; }
        [Required]
        public required ValueIds ValueIds { get; init; }
    }

    public class Ids
    {
        [Required]
        public required string Animal { get; init; }
    }

    public class ValueIds
    {
        [Required]
        public required string[] ForCats { get; init; }
        [Required]
        public required string[] ForDogs { get; init; }
        [Required]
        public required string[] ForOthers { get; init; }
    }
}
