namespace BagiraWebApi.Services.Exchanges.DataModels
{
    public class ExchangeGoodResult : ExchangeResult
    {
        public required List<int> IdsToUpdateImages { get; set; }
        public required List<int> IdsToDeleteImages { get; set; }
    }
}
