﻿
using BagiraServer.Services.Parser;
using BagiraWebApi.Services.Exchanges;

namespace BagiraWebApi.Services
{
    public class Worker: BackgroundService
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
            await Task.Delay(TimeSpan.FromMinutes(0), stoppingToken);
            try
            {
            //Exchange1c(stoppingToken);   
            _logger.LogInformation($"Start Worker: {DateTime.UtcNow.AddHours(5)} =====================");
            using var scope = _serviceProvider.CreateScope();
            var parser = scope.ServiceProvider.GetRequiredService<ParserService>();
            await parser.UpdateParserGoodsAsync();

            } catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private async void UpdateGoods(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Start Worker: {DateTime.UtcNow.AddHours(5)} =====================");
                    using var scope = _serviceProvider.CreateScope();
                    var exchange1C = scope.ServiceProvider.GetRequiredService<Exchange1C>();
                    await exchange1C.Update();
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
