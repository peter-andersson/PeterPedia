using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PeterPedia.Server.Services
{
    public class ConsumeRefreshArticleService : BackgroundService
    {
        private readonly ILogger<ConsumeRefreshArticleService> _logger;

        public ConsumeRefreshArticleService(IServiceProvider services, ILogger<ConsumeRefreshArticleService> logger)
        {
            Services = services;
            _logger = logger;
        }

        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ConsumeRefreshArticleService running.");

            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ConsumeRefreshArticleService is working.");

            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<RefreshArticleService>();

                await scopedProcessingService.DoWork(stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ConsumeRefreshArticleService is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}