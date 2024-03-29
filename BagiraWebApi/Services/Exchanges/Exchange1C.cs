using BagiraWebApi.Services.Bagira;
using BagiraWebApi.Services.Exchanges.ExchangeServices;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace BagiraWebApi.Services.Exchanges
{
    public class Exchange1C
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<Exchange1C> _logger;
        private readonly IOutputCacheStore _cache;

        private readonly ExchangeGoodService _exchangeGoodService;
        private readonly ExchangeStorageService _exchangeStorageService;
        private readonly ExchangeRestService _exchangeRestService;
        private readonly ExchangePriceService _exchangePriceService;
        private readonly ExchangePropertyValuesService _exchangePropertyValuesService;
        private readonly ExchangeImageService _exchangeImageService;

        public Exchange1C(
            IOptions<Connection1CConfig> options,
            ApplicationContext context,
            ILogger<Exchange1C> logger,
            IOutputCacheStore cache
            )
        {
            _context = context;
            _logger = logger;
            _cache = cache;

            var connection1CConfig = options.Value;
            var soap1C = new Soap1C(connection1CConfig);

            _exchangeGoodService = new ExchangeGoodService(context, soap1C, logger);
            _exchangeStorageService = new ExchangeStorageService(context, soap1C);
            _exchangeRestService = new ExchangeRestService(context, soap1C);
            _exchangePriceService = new ExchangePriceService(context, soap1C);
            _exchangePropertyValuesService = new ExchangePropertyValuesService(context, soap1C);
            _exchangeImageService = new ExchangeImageService(context, soap1C, logger);
        }

        public async Task UpdateAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _logger.LogInformation(">>>>>>>>>>>>>> Exchange 1c: Start");

            var updateGoodsResult = await _exchangeGoodService.UpdateAsync();
            var updateStoragesResult = await _exchangeStorageService.UpdateAsync();
            var updateRestsResult = await _exchangeRestService.UpdateAsync();
            var updatePriceTypesResult = await _exchangePriceService.UpdatePriceTypesAsync();
            var updatePricesResult = await _exchangePriceService.UpdatePricesAsync();
            var updatePropertyValuesResult = await _exchangePropertyValuesService.UpdateAsync();
            var updateImagesResult = await _exchangeImageService.UpdateAsync(updateGoodsResult);

            if (updateGoodsResult.CreatedCount > 0 || updateGoodsResult.UpdatedCount > 0 || updatePropertyValuesResult.WasChanged)
            {
                await _exchangeGoodService.UpdateKeywordsAsync(null, "");
                await _context.SaveChangesAsync();
            }

            if (updateGoodsResult.WasChanged || updatePricesResult.WasChanged
                || updateRestsResult.WasChanged || updatePropertyValuesResult.WasChanged)
            {
                await _cache.EvictByTagAsync("GoodsMenu", new CancellationToken());
                await _cache.EvictByTagAsync("Goods", new CancellationToken());
            }

            stopwatch.Stop();

            _logger.LogInformation($">>>>>>>>>>>>>> Exchange 1c: Done in {stopwatch.Elapsed.TotalSeconds}sec"
                + $"\nGoods:      {updateGoodsResult}"
                + $"\nStorages:   {updateStoragesResult}"
                + $"\nRests:      {updateRestsResult}"
                + $"\nPriceTypes: {updatePriceTypesResult}"
                + $"\nPrices:     {updatePricesResult}"
                + $"\nProperty:   {updatePropertyValuesResult}"
                + $"\nImages:     {updateImagesResult}"
                );
        }

        public async Task CheckImagesAsync()
        {
            try
            {
                _logger.LogInformation(">>>>>>>>>>>>>> Check Images: Start");
                var updateImagesResult = await _exchangeImageService.CheckImagesAsync();
                _logger.LogInformation($">>>>>>>>>>>>>> Check Images: Done in {updateImagesResult.ElapsedSec}sec"
                + $"\nAdd images:      {updateImagesResult.CreatedCount}"
                + $"\nDeleted images:  {updateImagesResult.DeletedCount}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("Cannot check images. Exception: {Ex}", ex);
            }
        }
    }
}
