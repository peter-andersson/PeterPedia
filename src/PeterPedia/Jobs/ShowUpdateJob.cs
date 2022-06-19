using Quartz;

namespace PeterPedia.Jobs;

[DisallowConcurrentExecution]
public partial class ShowUpdateJob : IJob
{
    private static readonly string s_JobName = "ShowUpdateJob";

    private readonly ILogger<ShowUpdateJob> _logger;
    private readonly ITVShows _tvShows;

    public ShowUpdateJob(ILogger<ShowUpdateJob> logger, ITVShows episodeManager)
    {
        _logger = logger;
        _tvShows = episodeManager;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        LogMessage.ExecuteJob(_logger, s_JobName);

        await _tvShows.RefreshAsync();

    }
}
