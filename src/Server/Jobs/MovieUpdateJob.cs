using Quartz;

namespace PeterPedia.Server.Jobs;

[DisallowConcurrentExecution]
public partial class MovieUpdateJob : IJob
{
    private static readonly string s_JobName = "MovieUpdateJob";

    private readonly ILogger<MovieUpdateJob> _logger;
    private readonly IMovieManager _movieManager;

    public MovieUpdateJob(
        ILogger<MovieUpdateJob> logger,
        IMovieManager movieManager)
    {
        _logger = logger;
        _movieManager = movieManager;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        LogMessage.ExecuteJob(_logger, s_JobName);

        await _movieManager.RefreshAsync();
    }    
}
