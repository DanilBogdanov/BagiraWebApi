using BagiraWebApi.Models.Bagira;
using BagiraWebApi.Services.Bagira;
using BagiraWebApi.Services.Exchanges.DataModels;
using BagiraWebApi.Services.Exchanges.DataModels.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static BagiraWebApi.Services.Exchanges.Comparators;
using static System.Net.Mime.MediaTypeNames;

namespace BagiraWebApi.Services.Exchanges.ExchangeServices
{
    public class ExchangeGoodService
    {
        const int STEP_LOAD_GOODS = 100;
        private readonly ApplicationContext _context;
        private readonly Soap1C _soap1C;
        private readonly ILogger<Exchange1C> _logger;
        private readonly KeywordsConfig _keywordsConfig;

        public ExchangeGoodService(
            ApplicationContext context,
            Soap1C soap1C, ILogger<Exchange1C> logger,
            KeywordsConfig keywords)
        {
            _context = context;
            _soap1C = soap1C;
            _logger = logger;
            _keywordsConfig = keywords;
        }

        public async Task UpdateKeywordsAsync(int? parentId, string parentKeywords)
        {
            var goods = await _context.Goods.Where(g => g.ParentId == parentId).ToListAsync();

            foreach (var good in goods)
            {
                if (good.IsGroup)
                {
                    good.KeyWords = $"{parentKeywords} {good.Name}";
                    await UpdateKeywordsAsync(good.Id, good.KeyWords);
                }
                else
                {
                    var keywords = $"{parentKeywords} {good.Name} {good.FullName}";

                    var animalProp = _context.GoodPropertyValues
                        .FirstOrDefault(gpv => gpv.PropertyId == "М00000007" && gpv.GoodId == good.Id);

                    if (animalProp != null)
                    {
                        var keyValue = _keywordsConfig.Animal.FirstOrDefault(pair => pair.Key == animalProp.ValueId);

                        if (keyValue != null)
                        {
                            keywords = $"{keywords} {keyValue.Value}";
                        }
                    }
                    string target = " ";

                    string bracketsPattern = @"[()]";
                    Regex bracketsRegex = new(bracketsPattern);
                    keywords = bracketsRegex.Replace(keywords, target);
                    
                    string spacesPattern = @"\s+";
                    Regex regex = new(spacesPattern);
                    keywords = bracketsRegex.Replace(keywords, target);

                    keywords = keywords.Trim();

                    good.KeyWords = keywords;
                }
            }
        }

        public async Task<ExchangeGoodResult> UpdateAsync()
        {
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

            var addGoodResult = await AddGoods(idsToAdd);
            var updateGoodResult = await UpdateGoods(idsToUpdate);
            var deleteGoodResult = await DeleteGoods(idsToDel);

            var idsToUpdateImage = new List<int>();
            var idsToDeleteImage = new List<int>();

            idsToUpdateImage.AddRange(addGoodResult.IdsToUpdateImage);
            idsToUpdateImage.AddRange(updateGoodResult.IdsToUpdateImage);

            idsToDeleteImage.AddRange(updateGoodResult.IdsToDeleteImage);
            idsToDeleteImage.AddRange(deleteGoodResult.IdsToDeleteImage);


            if (idsToAdd.Count > 0 || updateGoodResult.HasChangedParent)
            {
                var groups = _context.Goods.Where(good => good.IsGroup).AsNoTracking().ToList();
                await UpdatePath(parent: null, parentPath: "/", groups);
            }

            await _context.SaveChangesAsync();

            stopwatch.Stop();

            return new ExchangeGoodResult
            {
                ElapsedSec = stopwatch.Elapsed.TotalSeconds,
                CreatedCount = idsToAdd.Count,
                UpdatedCount = idsToUpdate.Count,
                DeletedCount = idsToDel.Count,
                IdsToUpdateImages = idsToUpdateImage,
                IdsToDeleteImages = idsToDeleteImage
            };
        }

        private async Task<UpdateGoodResult> AddGoods(List<int> ids)
        {
            var addGoodResult = new UpdateGoodResult();

            for (int i = 0; i < ids.Count; i += STEP_LOAD_GOODS)
            {
                var idsToLoad = ids.Skip(i).Take(STEP_LOAD_GOODS);
                var goods = await _soap1C.GetGoods(idsToLoad);

                foreach (var good in goods)
                {
                    if (good.ImgDataVersion != null)
                    {
                        addGoodResult.IdsToUpdateImage.Add(good.Id);
                    }
                }

                await _context.Goods.AddRangeAsync(goods);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Add {i + goods.Count} new goods of {ids.Count}");
            }

            return addGoodResult;
        }

        private async Task<UpdateGoodResult> UpdateGoods(List<int> ids)
        {
            var updateGoodResult = new UpdateGoodResult();

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
                        good.DataVersion = loadedGood.DataVersion;
                        if (good.ParentId != loadedGood.ParentId)
                        {
                            good.ParentId = loadedGood.ParentId;
                            updateGoodResult.HasChangedParent = true;
                        }
                        good.IsGroup = loadedGood.IsGroup;
                        good.Name = loadedGood.Name;
                        good.FullName = loadedGood.FullName;
                        good.Description = loadedGood.Description;
                        if (loadedGood.ImgDataVersion != good.ImgDataVersion)
                        {
                            if (loadedGood.ImgDataVersion != null)
                            {
                                updateGoodResult.IdsToUpdateImage.Add(good.Id);
                            }
                            else
                            {
                                updateGoodResult.IdsToDeleteImage.Add(good.Id);
                            }
                            good.ImgDataVersion = loadedGood.ImgDataVersion;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Updated {i + loadedGoods.Count} goods of {ids.Count}");
            }

            return updateGoodResult;
        }

        private async Task<UpdateGoodResult> DeleteGoods(List<int> ids)
        {
            var deleteGoodsResult = new UpdateGoodResult();

            if (ids.Count > 0)
            {
                var idsWithImage = _context.Goods.Where(g => ids.Contains(g.Id) && g.ImgDataVersion != null).Select(g => g.Id).ToList();
                deleteGoodsResult.IdsToDeleteImage.AddRange(idsWithImage);

                await _context.Goods.Where(good => ids.Contains(good.Id)).ExecuteDeleteAsync();
            }

            return deleteGoodsResult;
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

        private class UpdateGoodResult
        {
            public bool HasChangedParent { get; set; }
            public List<int> IdsToUpdateImage { get; set; } = new();
            public List<int> IdsToDeleteImage { get; set; } = new();
        }
    }
}
