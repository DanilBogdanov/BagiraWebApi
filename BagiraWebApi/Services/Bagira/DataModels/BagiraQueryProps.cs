namespace BagiraWebApi.Services.Bagira.DataModels
{
    public class BagiraQueryProps
    {
        public string? Query { get; set; }
        public int? GroupId { get; set; }
        public int? Take { get; set; }
        public int? Skip { get; set; }
        public string[]? PropertyValuesIds { get; set; }
    }
}
