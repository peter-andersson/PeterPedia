using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PeterPedia.Server.Services
{
    public class ConsumeRemoveArticleService : BackgroundService
    {
        private readonly ILogger<ConsumeRemoveArticleService> _logger;

        public ConsumeRemoveArticleService(IServiceProvider services, ILogger<ConsumeRemoveArticleService> logger)
        {
            Services = services;
            _logger = logger;
        }

        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ConsumeRemoveArticleService running.");

            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ConsumeRemoveArticleService is working.");

            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<RemoveArticleService>();

                await scopedProcessingService.DoWork(stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ConsumeRemoveArticleService is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}