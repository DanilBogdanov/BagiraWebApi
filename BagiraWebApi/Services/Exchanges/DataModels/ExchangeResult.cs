namespace BagiraWebApi.Services.Exchanges.DataModels
{
    public class ExchangeResult
    {
        public bool HasChangedParent { get; set; }
        public double ElapsedSec { get; set; }
        public int CreatedCount {  get; set; }
        public int UpdatedCount { get; set; }
        public int DeletedCount { get; set; }
        public bool WasChanged
        {
            get
            {
                return CreatedCount >0 || UpdatedCount > 0 || DeletedCount >0;
            }
        }

        public override string? ToString()
        {
            return $"Add:{CreatedCount},\tUpdate:{UpdatedCount},\tDel:{DeletedCount},\tTime:{ElapsedSec}sec";
        }
    }
}
