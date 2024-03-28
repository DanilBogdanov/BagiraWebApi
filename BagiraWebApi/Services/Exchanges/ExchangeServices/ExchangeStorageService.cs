using BagiraWebApi.Services.Exchanges.DataModels;
using Microsoft.EntityFrameworkCore;
using static BagiraWebApi.Services.Exchanges.Comparators;
using System.Diagnostics;

namespace BagiraWebApi.Services.Exchanges.ExchangeServices
{
    public class ExchangeStorageService
    {
        private readonly ApplicationContext _context;
        private readonly Soap1C _soap1C; 

        public ExchangeStorageService(ApplicationContext context, Soap1C soap1C) {
            _context = context;
            _soap1C = soap1C;
        }

        public async Task<ExchangeResult> UpdateAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var loadedStorages = await _soap1C.GetGoodStorages();
            var dbStorages = _context.GoodStorages.AsNoTracking().ToList();

            var toAdd = loadedStorages.Except(dbStorages, new GoodStorageIdComparator()).ToList();
            await _context.GoodStorages.AddRangeAsync(toAdd);

            var toDel = dbStorages.Except(loadedStorages, new GoodStorageIdComparator()).ToList();
            _context.GoodStorages.RemoveRange(toDel);

            var toUpdate = loadedStorages.Intersect(dbStorages, new GoodStorageIdComparator())
                .Except(dbStorages, new GoodStorageNameComparator()).ToList();
            _context.GoodStorages.UpdateRange(toUpdate);

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

    }
}
