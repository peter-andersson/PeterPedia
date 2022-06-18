namespace PeterPedia.Services;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used by source generator")]
public static partial class LogMessage
{
    [LoggerMessage(0, LogLevel.Error, "Failed to fetch data from themoviedb.org")]
    public static partial void TheMovieDbFailed(ILogger logger);

    [LoggerMessage(1, LogLevel.Debug, "Add movie {id}")]
    public static partial void MovieAdd(ILogger logger, int id);

    [LoggerMessage(2, LogLevel.Error, "Failed to add movie {id}, error {message}")]
    public static partial void MovieAddFailed(ILogger logger, int id, string message);

    [LoggerMessage(3, LogLevel.Debug, "Update movie {movie}")]
    public static partial void MovieUpdate(ILogger logger, Movie movie);

    [LoggerMessage(4, LogLevel.Error, "Failed to update movie {movie}, error {message}")]
    public static partial void MovieUpdateFailed(ILogger logger, Movie movie, string message);

    [LoggerMessage(6, LogLevel.Debug, "Delete movie with id {id}")]
    public static partial void MovieDelete(ILogger logger, int id);

    [LoggerMessage(7, LogLevel.Error, "Failed to delete movie with id {id}, error {message}")]
    public static partial void MovieDeleteFailed(ILogger logger, int id, string message);

    [LoggerMessage(8, LogLevel.Error, "Failed to download from {url}.")]
    public static partial void FailedDownload(ILogger logger, string url, Exception e);

    [LoggerMessage(9, LogLevel.Debug, "Execute job {job}.")]
    public static partial void ExecuteJob(ILogger logger, string job);

    [LoggerMessage(10, LogLevel.Information, "Movie not changed, {movie}")]
    public static partial void MovieNotChanged(ILogger logger, MovieEF movie);    

    [LoggerMessage(23, LogLevel.Debug, "Downloading image from {url} to {filename}")]
    public static partial void DownloadImage(ILogger logger, string url, string filename);

    [LoggerMessage(24, LogLevel.Error, "Failed to download image from {url} to {filename}.")]
    public static partial void DownloadImageFailed(ILogger logger, string url, string filename, Exception e);

    [LoggerMessage(25, LogLevel.Debug, "Add tv show {id}")]
    public static partial void EpisodeAdd(ILogger logger, int id);

    [LoggerMessage(26, LogLevel.Error, "Failed to add show {showId}, error {message}")]
    public static partial void EpisodeAddFailed(ILogger logger, int showId, string message);

    [LoggerMessage(27, LogLevel.Debug, "Update show {show}")]
    public static partial void EpisodeUpdate(ILogger logger, Show show);

    [LoggerMessage(28, LogLevel.Error, "Failed to update show {show}, error {message}")]
    public static partial void EpisodeUpdateFailed(ILogger logger, Show show, string message);

    [LoggerMessage(29, LogLevel.Debug, "Delete show with id {id}")]
    public static partial void EpisodeDelete(ILogger logger, int id);

    [LoggerMessage(30, LogLevel.Error, "Failed to delete show with id {id}, error {message}")]
    public static partial void EpisodeDeleteFailed(ILogger logger, int id, string message);

    [LoggerMessage(31, LogLevel.Debug, "Episode update watch on {watch}.")]
    public static partial void EpisodeWatch(ILogger logger, ShowWatchData watch);

    [LoggerMessage(32, LogLevel.Error, "Episode watch update failed, data {watch}, error {message}.")]
    public static partial void EpisodeWatchFailed(ILogger logger, ShowWatchData watch, string message);

    [LoggerMessage(33, LogLevel.Debug, "Read article: {article}")]
    public static partial void ReaderReadArticle(ILogger logger, ArticleEF article);

    [LoggerMessage(34, LogLevel.Debug, "Video with id {id} not found.")]
    public static partial void VideoNotFound(ILogger logger, int id);

    [LoggerMessage(35, LogLevel.Information, "Video {title}, path {path} have been deleted.")]
    public static partial void VideoDeleted(ILogger logger, string title, string path);

    [LoggerMessage(36, LogLevel.Information, "Photo with path '{path}' no longer exists. Removing photo.")]
    public static partial void PhotoRemove(ILogger logger, string path);

    [LoggerMessage(37, LogLevel.Information, "Adding new album '{album}'")]
    public static partial void PhotoNewAlbum(ILogger logger, string album);

    [LoggerMessage(38, LogLevel.Information, "Adding new photo '{file}'")]
    public static partial void PhotoNew(ILogger logger, string file);

    [LoggerMessage(39, LogLevel.Debug, "Adding transaction, date: {date}, note1: {note1}, note2: {note2}, amount: {amount}")]
    public static partial void TransactionAdd(ILogger logger, DateTime date, string note1, string note2, double amount);

    [LoggerMessage(40, LogLevel.Information, "Duplicate transaction, date: {date}, note1: {note1}, note2: {note2}, amount: {amount}")]
    public static partial void TransactionExists(ILogger logger, DateTime date, string note1, string note2, double amount);

    [LoggerMessage(41, LogLevel.Error, "Reader subscription error: {message}")]
    public static partial void SubscriptionError(ILogger logger, string message);

    [LoggerMessage(42, LogLevel.Error, "{message}")]
    public static partial void ReaderException(ILogger logger, string message, Exception ex);

    [LoggerMessage(43, LogLevel.Error, "Exception when fetching feed data for subscription {title}")]
    public static partial void ReaderFeedException(ILogger logger, string title, Exception ex);

    [LoggerMessage(44, LogLevel.Error, "'{url}' is not a valid url.")]
    public static partial void ReaderInvalidUrl(ILogger logger, string url);

    [LoggerMessage(46, LogLevel.Debug, "UpdateSubscription - {title}")]
    public static partial void ReaderUpdateFeed(ILogger logger, string title);

    [LoggerMessage(47, LogLevel.Debug, "Feed has not changed.")]
    public static partial void ReaderFeedNotChanged(ILogger logger);

    [LoggerMessage(48, LogLevel.Information, "Adding new article {article} to subscription {subscription}")]
    public static partial void ReaderAddArticle(ILogger logger, string article, string subscription);
}
