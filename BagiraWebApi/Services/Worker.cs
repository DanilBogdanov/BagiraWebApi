
using BagiraWebApi.Services.Exchanges;

namespace BagiraWebApi.Services
{
    public class Worker: BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider) 
        {
            _serviceProvider = serviceProvider;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Start Worker =====================");
            using (var scope = _serviceProvider.CreateScope())
            {
                var exchange1C = scope.ServiceProvider.GetRequiredService<Exchange1C>();
                await exchange1C.Update();
            }
            await Task.Delay(5000, stoppingToken);
        }
    }
}
