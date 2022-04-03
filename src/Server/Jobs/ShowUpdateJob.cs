using Quartz;

namespace PeterPedia.Server.Jobs;

[DisallowConcurrentExecution]
public partial class ShowUpdateJob : IJob
{
    private static readonly string s_JobName = "ShowUpdateJob";

    private readonly ILogger<ShowUpdateJob> _logger;
    private readonly IEpisodeManager _episodeManager;

    public ShowUpdateJob(ILogger<ShowUpdateJob> logger, IEpisodeManager episodeManager)
    {
        _logger = logger;
        _episodeManager = episodeManager;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        LogMessage.ExecuteJob(_logger, s_JobName);

        await _episodeManager.RefreshAsync();

    }
}
