
using BagiraServer.Services.Parser;
using BagiraWebApi.Services.Exchanges;

namespace BagiraWebApi.Services
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Worker> _logger;

        public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // var updateGoodTask = UpdateGoods(stoppingToken);
            // var updateParserGoodsTask = UpdateParserGoods(stoppingToken);

            // await Task.WhenAll(updateGoodTask, updateParserGoodsTask);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Start Worker - Update Goods: {DateTime.UtcNow.AddHours(5)} =====================");
                    using var scope = _serviceProvider.CreateScope();
                    var exchange1C = scope.ServiceProvider.GetRequiredService<Exchange1C>();
                    await exchange1C.UpdateAsync();
                    _logger.LogInformation($"Stop Worker - Update Goods: {DateTime.UtcNow.AddHours(5)} =====================");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                try
                {
                    _logger.LogInformation($"Start Worker - Check Images: {DateTime.UtcNow.AddHours(5)} =====================");
                    using var scope = _serviceProvider.CreateScope();
                    var exchange1C = scope.ServiceProvider.GetRequiredService<Exchange1C>();
                    await exchange1C.CheckImagesAsync();
                    _logger.LogInformation($"Stop Worker - Check Images: {DateTime.UtcNow.AddHours(5)} =====================");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                try
                {
                    _logger.LogInformation($"Start Worker - Parser: {DateTime.UtcNow.AddHours(5)} =====================");
                    using var scope = _serviceProvider.CreateScope();
                    var parser = scope.ServiceProvider.GetRequiredService<ParserService>();
                    await parser.UpdateParserGoodsAsync();
                    _logger.LogInformation($"Stop Worker - Parser: {DateTime.UtcNow.AddHours(5)} =====================");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task UpdateGoods(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Start Worker - Update Goods: {DateTime.UtcNow.AddHours(5)} =====================");
                    using var scope = _serviceProvider.CreateScope();
                    var exchange1C = scope.ServiceProvider.GetRequiredService<Exchange1C>();
                    await exchange1C.UpdateAsync();
                    _logger.LogInformation($"Stop Worker - Update Goods: {DateTime.UtcNow.AddHours(5)} =====================");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task UpdateParserGoods(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Start Worker - Parser: {DateTime.UtcNow.AddHours(5)} =====================");
                    using var scope = _serviceProvider.CreateScope();
                    var parser = scope.ServiceProvider.GetRequiredService<ParserService>();
                    await parser.UpdateParserGoodsAsync();
                    _logger.LogInformation($"Stop Worker - Parser: {DateTime.UtcNow.AddHours(5)} =====================");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
