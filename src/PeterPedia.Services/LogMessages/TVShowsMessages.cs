namespace PeterPedia.Services.LogMessages;

public static partial class TVShowsMessages
{
    [LoggerMessage(1, LogLevel.Debug, "Add tv show {id}")]
    public static partial void Add(ILogger logger, int id);

    [LoggerMessage(2, LogLevel.Debug, "Added tv show {id}, title {title}")]
    public static partial void Added(ILogger logger, int id, string title);

    [LoggerMessage(3, LogLevel.Error, "Failed to add show {showId}, error \"{message}\"")]
    public static partial void AddFailed(ILogger logger, int showId, string message);

    [LoggerMessage(4, LogLevel.Debug, "Update show {showId}")]
    public static partial void Update(ILogger logger, int showId);

    [LoggerMessage(5, LogLevel.Debug, "Updated show {showId}")]
    public static partial void Updated(ILogger logger, int showId);

    [LoggerMessage(6, LogLevel.Error, "Failed to update show {showId}, error \"{message}\"")]
    public static partial void UpdateFailed(ILogger logger, int showId, string message);

    [LoggerMessage(7, LogLevel.Debug, "Delete show with id {id}")]
    public static partial void Delete(ILogger logger, int id);

    [LoggerMessage(8, LogLevel.Debug, "Deleted show with id {id}")]
    public static partial void Deleted(ILogger logger, int id);

    [LoggerMessage(9, LogLevel.Error, "Failed to delete show with id {id}, error \"{message}\"")]
    public static partial void DeleteFailed(ILogger logger, int id, string message);

    [LoggerMessage(10, LogLevel.Debug, "Episode update watch state on season: {seasonId}, episode {episodeId}, state {state}.")]
    public static partial void Watch(ILogger logger, int? seasonId, int? episodeId, bool state);

    [LoggerMessage(11, LogLevel.Error, "Episode watch update failed, season: {seasonId}, episode {episodeId}, state {state}, error \"{message}\".")]
    public static partial void WatchFailed(ILogger logger, int? seasonId, int? episodeId, bool state, string message);

    [LoggerMessage(12, LogLevel.Error, "Failed to fetch data from themoviedb.org")]
    public static partial void TheMovieDbFailed(ILogger logger);
}
