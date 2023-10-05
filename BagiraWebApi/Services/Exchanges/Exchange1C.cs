using BagiraWebApi.Models.Bagira;
using BagiraWebApi.Services.Exchanges.DataModels;
using BagiraWebApi.Services.Exchanges.DataModels.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static BagiraWebApi.Services.Exchanges.Comparators;

namespace BagiraWebApi.Services.Exchanges
{
    public class Exchange1C
    {
        const int STEP_LOAD_GOODS = 100;
        const string IMG_FOLDER = "bagira/img";
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
            await UpdatePrices();
        }

        private async Task UpdateGoods()
        {
            _logger.LogInformation($"Start Load DataVersion");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var loadedGoodDataVersions = (await _soap1C.GetGoodsDataVersions())
                .Select(gdv => new GoodDataVersionDTO { Id = gdv.Id, DataVersion = gdv.DataVersion });
            var dbGoodDataVersions = _context.Goods.Select(good => new GoodDataVersionDTO
            { Id = good.Id, DataVersion = good.DataVersion }).AsNoTracking().ToList();

            var idsToAdd = loadedGoodDataVersions.Except(dbGoodDataVersions, new GoodIdComparator()).Select(dto => dto.Id).ToList();
            var idsToUpdate = dbGoodDataVersions.Intersect(loadedGoodDataVersions, new GoodIdComparator())
                .Except(loadedGoodDataVersions, new GoodDataVersionComparator()).Select(dto => dto.Id).ToList();
            var idsToDel = dbGoodDataVersions.Except(loadedGoodDataVersions, new GoodIdComparator()).Select(dto => dto.Id).ToList();

            await AddGoods(idsToAdd);
            var exchangeResult = await UpdateGoods(idsToUpdate);
            if (idsToDel.Count > 0)
            {
                await DeleteGoods(idsToDel);
            }

            if (idsToAdd.Count > 0 || exchangeResult.HasChangedParent)
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

                foreach (var good in goods)
                {
                    if (good.ImgDataVersion != null && good.ImgExt != null)
                    {
                        good.ImgUrl = await UpdateImg(good.Id, good.ImgExt);
                    }
                }
                await _context.Goods.AddRangeAsync(goods);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Add {i + goods.Count} new goods of {ids.Count}");
            }
        }

        private async Task<ExchangeResult> UpdateGoods(List<int> ids)
        {
            bool hasChangedParent = false;

            for (int i = 0; i < ids.Count; i += STEP_LOAD_GOODS)
            {
                var idsToLoad = ids.Skip(i).Take(STEP_LOAD_GOODS);
                var loadedGoods = await _soap1C.GetGoods(idsToLoad);
                var dbGoods = _context.Goods.Where(good => idsToLoad.Contains(good.Id));
                foreach (var good in dbGoods)
                {
                    var loadedGood = loadedGoods.Find(g => g.Id == good.Id);
                    if (loadedGood != null)
                    {
                        Console.WriteLine(good.Name);
                        good.DataVersion = loadedGood.DataVersion;
                        if (good.ParentId != loadedGood.ParentId)
                        {
                            good.ParentId = loadedGood.ParentId;
                            hasChangedParent = true;
                        }
                        good.IsGroup = loadedGood.IsGroup;
                        good.Name = loadedGood.Name;
                        good.FullName = loadedGood.FullName;
                        good.Description = loadedGood.Description;
                        if (loadedGood.ImgDataVersion != good.ImgDataVersion)
                        {
                            if (loadedGood.ImgDataVersion != null && loadedGood.ImgExt != null)
                            {
                                var imgUrl = await UpdateImg(loadedGood.Id, loadedGood.ImgExt);
                                if (good.ImgUrl != imgUrl)
                                {
                                    DeleteImg(good.ImgUrl);
                                    good.ImgUrl = imgUrl;
                                }
                            }
                            else
                            {
                                DeleteImg(good.ImgUrl);
                                good.ImgUrl = null;
                            }
                            good.ImgDataVersion = loadedGood.ImgDataVersion;
                            good.ImgExt = loadedGood.ImgExt;
                        }
                    }
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Updated {i + loadedGoods.Count} goods of {ids.Count}");
            }

            return new ExchangeResult
            {
                HasChangedParent = hasChangedParent,
            };
        }

        private async Task DeleteGoods(List<int> ids)
        {
            var imgUrls = _context.Goods.Where(g =>  ids.Contains(g.Id) && g.ImgUrl != null).Select(g => g.ImgUrl);
            foreach(var imgUrl in imgUrls)
            {
                DeleteImg(imgUrl);
            }
            await _context.Goods.Where(good => ids.Contains(good.Id)).ExecuteDeleteAsync();
        }

        private async Task<string?> UpdateImg(int id, string imgExt)
        {
            var imgBinary = await _soap1C.GetImage(id);
            if (imgBinary != "")
            {
                if (imgExt == "")
                {
                    imgExt = ".jpg";
                }
                string filePath = $"{IMG_FOLDER}/{id}{imgExt}";
                using (BinaryWriter writer = new BinaryWriter(File.Open($"wwwroot/{filePath}", FileMode.Create)))
                {
                    writer.Write(Convert.FromBase64String(imgBinary));
                }
                return filePath;
            }
            return null;
        }

        private void DeleteImg(string? filePath)
        {
            if (filePath != null)
            {
                File.Delete($"wwwroot/{filePath}");
            }
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

        private async Task UpdatePrices()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var loadedGoodPrices = await _soap1C.GetPrices();
            var dbGoodPrices = _context.GoodPrices.AsNoTracking().ToList();
            var idComparator = new GoodPriceIdComparator();

            //Add
            var itemsToAdd = loadedGoodPrices.Except(dbGoodPrices, idComparator).ToList();
            await _context.AddRangeAsync(itemsToAdd);
            
            //Delete
            var itemsToDel = dbGoodPrices.Except(loadedGoodPrices, idComparator).ToList();
            _context.RemoveRange(itemsToDel);
            

            //Update
            var loadedToUpdate = loadedGoodPrices.Intersect(dbGoodPrices, idComparator).ToList();
            var dbToUpdate = dbGoodPrices.Intersect(loadedGoodPrices, idComparator).ToList();
            var itemsToUpdate = loadedToUpdate.Except(dbToUpdate, new GoodPriceFullComparator()).ToList();
            _context.UpdateRange(itemsToUpdate);
            
            _context.SaveChanges();
            stopwatch.Stop();

            _logger.LogInformation($"\nUpdated Prices in {stopwatch.Elapsed.TotalSeconds} sec");
        }
    }
}
