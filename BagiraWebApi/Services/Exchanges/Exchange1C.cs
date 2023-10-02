using BagiraWebApi.Models.Bagira;
using BagiraWebApi.Services.Exchanges.DataModels.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using static BagiraWebApi.Services.Exchanges.Comparators;

namespace BagiraWebApi.Services.Exchanges
{
    public class Exchange1C
    {
        const int STEP_LOAD_GOODS = 500;
        private readonly ILogger<Exchange1C> _logger;
        private readonly Soap1C _soap1C;
        private readonly ApplicationContext _context;

        public Exchange1C(ILogger<Exchange1C> logger, IConfiguration configuration, ApplicationContext context)
        {
            _logger = logger;
            _soap1C = new(configuration);
            _context = context;
        }

        public async Task Update()
        {

            Console.WriteLine("Start update");

            await UpdateGoods();
            await UpdateStorages();
            await UpdatePriceTypes();
        }

        private async Task UpdateGoods()
        {
            _logger.LogInformation($"Start Load DataVersion");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var loadedGoodDataVersions = await _soap1C.GetGoodsDataVersions();
            var dbGoodDataVersions = _context.Goods.Select(good => new GoodDataVersionDTO
            { Id = good.Id, DataVersion = good.DataVersion }).AsNoTracking().ToList();

            var idsToAdd = loadedGoodDataVersions.Except(dbGoodDataVersions, new GoodIdComparator()).Select(dto => dto.Id).ToList();
            var idsToUpdate = dbGoodDataVersions.Intersect(loadedGoodDataVersions, new GoodIdComparator())
                .Except(loadedGoodDataVersions, new GoodDataVersionComparator()).Select(dto => dto.Id).ToList();
            var idsToDel = dbGoodDataVersions.Except(loadedGoodDataVersions, new GoodIdComparator()).Select(dto => dto.Id).ToList();

            await AddGoods(idsToAdd);
            await UpdateGoods(idsToUpdate);
            if (idsToDel.Count > 0)
            {
                await DeleteGoods(idsToDel);
            }

            if (idsToAdd.Count + idsToUpdate.Count > 0)
            {
                var groups = _context.Goods.Where(good => good.IsGroup).AsNoTracking().ToList();
                await UpdatePath(parent: null, parentPath: "/", groups);
            }

            await _context.SaveChangesAsync();

            stopwatch.Stop();
            Console.WriteLine($"Stop update: {stopwatch.Elapsed.TotalSeconds}sec");
        }

        private async Task AddGoods(List<int> ids)
        {
            for (int i = 0; i < ids.Count; i += STEP_LOAD_GOODS)
            {
                var idsToLoad = ids.Skip(i).Take(STEP_LOAD_GOODS);
                var goods = await _soap1C.GetGoods(idsToLoad);
                await _context.Goods.AddRangeAsync(goods);
                _logger.LogInformation($"Add {i + goods.Count} new goods of {ids.Count}");
            }
        }

        private async Task UpdateGoods(List<int> ids)
        {
            for (int i = 0; i < ids.Count; i += STEP_LOAD_GOODS)
            {
                var idsToLoad = ids.Skip(i).Take(STEP_LOAD_GOODS);
                var goods = await _soap1C.GetGoods(idsToLoad);
                goods.ForEach(good => Console.WriteLine(">>>>" + good.Name));
                _context.Goods.UpdateRange(goods);
                _logger.LogInformation($"Updated {i + goods.Count} goods of {ids.Count}");
            }
        }

        private async Task DeleteGoods(List<int> ids)
        {
            await _context.Goods.Where(good => ids.Contains(good.Id)).ExecuteDeleteAsync();
        }

        private async Task UpdatePath(int? parent, string parentPath, List<Good> groups)
        {
            await _context.Goods.Where(good => good.ParentId == parent)
                .ExecuteUpdateAsync(good => good.SetProperty(g => g.Path, parentPath));

            foreach (var item in groups.Where(group => group.ParentId == parent))
            {
                await UpdatePath(item.Id, $"{parentPath}{item.Id}/", groups);
            }
        }

        private async Task UpdateStorages()
        {
            var loadedStorages = await _soap1C.GetGoodStorages();
            var dbStorages = _context.GoodStorages.AsNoTracking().ToList();

            var toAdd = loadedStorages.Except(dbStorages, new GoodStorageIdComparator());
            await _context.GoodStorages.AddRangeAsync(toAdd);

            var toDel = dbStorages.Except(loadedStorages, new GoodStorageIdComparator());
            _context.GoodStorages.RemoveRange(toDel);

            var toUpdate = loadedStorages.Intersect(dbStorages, new GoodStorageIdComparator())
                .Except(dbStorages, new GoodStorageNameComparator());
            _context.GoodStorages.UpdateRange(toUpdate);

            await _context.SaveChangesAsync();
        }

        private async Task UpdatePriceTypes()
        {
            var loadedPriceTypes = await _soap1C.GetPriceTypes();
            var dbPriceTypes = _context.GoodPriceTypes.AsNoTracking().ToList();

            var toAdd = loadedPriceTypes.Except(dbPriceTypes, new GoodPriceTypeIdComparator());
            await _context.GoodPriceTypes.AddRangeAsync(toAdd);

            var toDel = dbPriceTypes.Except(loadedPriceTypes, new GoodPriceTypeIdComparator());
            _context.GoodPriceTypes.RemoveRange(toDel);

            var toUpdate = loadedPriceTypes.Intersect(dbPriceTypes, new GoodPriceTypeIdComparator())
                .Except(dbPriceTypes, new GoodPriceTypeNameComparator());
            _context.GoodPriceTypes.UpdateRange(toUpdate);

            await _context.SaveChangesAsync();
        }
    }
}
