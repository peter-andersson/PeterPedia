using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PeterPedia.Server.Services
{
    public class ConsumeShowUpdateService : BackgroundService
    {
        private readonly ILogger<ConsumeShowUpdateService> _logger;

        public ConsumeShowUpdateService(IServiceProvider services, ILogger<ConsumeShowUpdateService> logger)
        {
            Services = services;
            _logger = logger;
        }

        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ConsumeShowUpdateService running.");

            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ConsumeShowUpdateService is working.");

            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IShowUpdateService>();

                await scopedProcessingService.DoWork(stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ConsumeShowUpdateService is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}