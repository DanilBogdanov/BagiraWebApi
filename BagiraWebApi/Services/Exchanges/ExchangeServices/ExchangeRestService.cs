using BagiraWebApi.Services.Exchanges.DataModels;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using static BagiraWebApi.Services.Exchanges.Comparators;

namespace BagiraWebApi.Services.Exchanges.ExchangeServices
{
    public class ExchangeRestService
    {
        private readonly ApplicationContext _context;
        private readonly Soap1C _soap1C;

        public ExchangeRestService(ApplicationContext context, Soap1C soap1C)
        {
            _context = context;
            _soap1C = soap1C;
        }

        public async Task<ExchangeResult> UpdateAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var loadedGoodRests = await _soap1C.GetRestOfGoods();
            var dbGoodRests = _context.GoodRests.AsNoTracking().ToList();

            //Add
            var itemsToAdd = loadedGoodRests.Except(dbGoodRests, new GoodRestIdComparator()).ToList();
            _context.AddRange(itemsToAdd);

            //Delete
            var itemsToDel = dbGoodRests.Except(loadedGoodRests, new GoodRestIdComparator()).ToList();
            _context.RemoveRange(itemsToDel);

            //Update
            var loadedToUpdate = loadedGoodRests.Intersect(dbGoodRests, new GoodRestIdComparator()).ToList();
            var dbToUpdate = dbGoodRests.Intersect(loadedToUpdate, new GoodRestIdComparator()).ToList();
            var itemsToUpdate = loadedToUpdate.Except(dbToUpdate, new GoodRestFullComparator()).ToList();
            _context.UpdateRange(itemsToUpdate);

            await _context.SaveChangesAsync();
            stopwatch.Stop();

            return new ExchangeResult
            {
                ElapsedSec = stopwatch.Elapsed.TotalSeconds,
                CreatedCount = itemsToAdd.Count,
                UpdatedCount = itemsToUpdate.Count,
                DeletedCount = itemsToDel.Count
            };
        }
    }
}
