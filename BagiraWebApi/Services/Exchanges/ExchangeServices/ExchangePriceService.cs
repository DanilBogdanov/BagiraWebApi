using BagiraWebApi.Services.Exchanges.DataModels;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using static BagiraWebApi.Services.Exchanges.Comparators;

namespace BagiraWebApi.Services.Exchanges.ExchangeServices
{
    public class ExchangePriceService
    {
        private readonly ApplicationContext _context;
        private readonly Soap1C _soap1C;

        public ExchangePriceService(ApplicationContext context, Soap1C soap1C)
        {
            _context = context;
            _soap1C = soap1C;
        }

        public async Task<ExchangeResult> UpdatePriceTypesAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var loadedPriceTypes = await _soap1C.GetPriceTypes();
            var dbPriceTypes = _context.GoodPriceTypes.AsNoTracking().ToList();

            var toAdd = loadedPriceTypes.Except(dbPriceTypes, new GoodPriceTypeIdComparator()).ToList();
            await _context.GoodPriceTypes.AddRangeAsync(toAdd);

            var toDel = dbPriceTypes.Except(loadedPriceTypes, new GoodPriceTypeIdComparator()).ToList();
            _context.GoodPriceTypes.RemoveRange(toDel);

            var toUpdate = loadedPriceTypes.Intersect(dbPriceTypes, new GoodPriceTypeIdComparator())
                .Except(dbPriceTypes, new GoodPriceTypeNameComparator()).ToList();
            _context.GoodPriceTypes.UpdateRange(toUpdate);

            await _context.SaveChangesAsync();
            stopwatch.Stop();

            return new ExchangeResult
            {
                ElapsedSec = stopwatch.Elapsed.TotalSeconds,
                CreatedCount = toAdd.Count,
                UpdatedCount = toUpdate.Count,
                DeletedCount = toDel.Count
            };
        }

        public async Task<ExchangeResult> UpdatePricesAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var loadedGoodPrices = await _soap1C.GetPrices();
            var dbGoodPrices = _context.GoodPrices.AsNoTracking().ToList();
            var idComparator = new GoodPriceIdComparator();

            var itemsToAdd = loadedGoodPrices.Except(dbGoodPrices, idComparator).ToList();
            await _context.AddRangeAsync(itemsToAdd);

            var itemsToDel = dbGoodPrices.Except(loadedGoodPrices, idComparator).ToList();
            _context.RemoveRange(itemsToDel);


            var loadedToUpdate = loadedGoodPrices.Intersect(dbGoodPrices, idComparator).ToList();
            var dbToUpdate = dbGoodPrices.Intersect(loadedGoodPrices, idComparator).ToList();
            var itemsToUpdate = loadedToUpdate.Except(dbToUpdate, new GoodPriceFullComparator()).ToList();
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
