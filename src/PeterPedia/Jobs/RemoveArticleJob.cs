using Microsoft.EntityFrameworkCore;
using Quartz;

namespace PeterPedia.Jobs;

[DisallowConcurrentExecution]
public partial class RemoveArticleJob : IJob
{
#pragma warning disable IDE0052 // Remove unread private members
    private readonly ILogger<RemoveArticleJob> _logger;
#pragma warning restore IDE0052 // Remove unread private members
    private readonly PeterPediaContext _dbContext;

    public RemoveArticleJob(ILogger<RemoveArticleJob> logger, PeterPediaContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        LogExecute();

        DateTime ageLimit = DateTime.UtcNow.AddDays(-30);
        var articles = await _dbContext.Articles.Where(a => a.ReadDate < ageLimit).AsTracking().ToListAsync();

        LogRemoveArticle(articles.Count);

        _dbContext.Articles.RemoveRange(articles);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }   

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter
    [LoggerMessage(0, LogLevel.Debug, "RemoveArticleJob - Execute")]
    partial void LogExecute();

    [LoggerMessage(1, LogLevel.Information, "Removing {count} articles.")]
    partial void LogRemoveArticle(int count);
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression
}
