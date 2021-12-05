using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PeterPedia.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PeterPedia.Server.Services
{
    internal class RemoveArticleService
    {
        private readonly ILogger<RemoveArticleService> _logger;
        private readonly PeterPediaContext _dbContext;

        public RemoveArticleService(ILogger<RemoveArticleService> logger, PeterPediaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Execute();

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        private async Task Execute()
        {
            _logger.LogInformation("RemoveArticleService - Execute");

            DateTime ageLimit = DateTime.UtcNow.AddDays(-30);
            var articles = await _dbContext.Articles.Where(a => a.ReadDate < ageLimit).AsTracking().ToListAsync();

            _logger.LogInformation($"Removing {articles.Count} articles.");

            _dbContext.Articles.RemoveRange(articles);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}