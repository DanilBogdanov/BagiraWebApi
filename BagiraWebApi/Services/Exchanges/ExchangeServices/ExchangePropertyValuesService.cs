using BagiraWebApi.Services.Exchanges.DataModels;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using static BagiraWebApi.Services.Exchanges.Comparators;

namespace BagiraWebApi.Services.Exchanges.ExchangeServices
{
    public class ExchangePropertyValuesService
    {
        private readonly ApplicationContext _context;
        private readonly Soap1C _soap1C;

        public ExchangePropertyValuesService(ApplicationContext context, Soap1C soap1C)
        {
            _context = context;
            _soap1C = soap1C;
        }

        public async Task<ExchangeResult> UpdateAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var loadedPropertyValues = await _soap1C.GetPropertyValues();
            var dbPropertyValues = _context.GoodPropertyValues.AsNoTracking().ToList();

            var itemsToAdd = loadedPropertyValues.Except(dbPropertyValues, new GoodPropertyValueIdComparator()).ToList();
            _context.AddRange(itemsToAdd);

            var itemsToDel = dbPropertyValues.Except(loadedPropertyValues, new GoodPropertyValueIdComparator()).ToList();
            _context.RemoveRange(itemsToDel);

            var loadedToUpdate = loadedPropertyValues.Intersect(dbPropertyValues, new GoodPropertyValueIdComparator()).ToList();
            var dbToUpdate = dbPropertyValues.Intersect(loadedToUpdate, new GoodPropertyValueIdComparator()).ToList();
            var itemsToUpdate = loadedToUpdate.Except(dbToUpdate, new GoodPropertyValueFullComparator()).ToList();
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
