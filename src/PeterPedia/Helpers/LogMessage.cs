namespace PeterPedia.Helpers;

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

    [LoggerMessage(11, LogLevel.Debug, "Adding new author {author}")]
    public static partial void AuthorAdd(ILogger logger, Author author);

    [LoggerMessage(12, LogLevel.Error, "Failed to add {author}, error {message}.")]
    public static partial void AuthorAddFailed(ILogger logger, Author author, string message);

    [LoggerMessage(13, LogLevel.Debug, "Delete author with id {id}.")]
    public static partial void AuthorDelete(ILogger logger, int id);

    [LoggerMessage(14, LogLevel.Error, "Failed to delete author with id {id}, error {message}.")]
    public static partial void AuthorDeleteFailed(ILogger logger, int id, string message);

    [LoggerMessage(15, LogLevel.Debug, "Update author {author}")]
    public static partial void AuthorUpdate(ILogger logger, Author author);

    [LoggerMessage(16, LogLevel.Error, "Failed to update {author}, error {message}.")]
    public static partial void AuthorUpdateFailed(ILogger logger, Author author, string message);

    [LoggerMessage(17, LogLevel.Debug, "Adding new book {book}")]
    public static partial void BookAdd(ILogger logger, Book book);

    [LoggerMessage(19, LogLevel.Debug, "Delete book with id {id}.")]
    public static partial void BookDelete(ILogger logger, int id);

    [LoggerMessage(20, LogLevel.Error, "Failed to delete book with id {id}, error {message}")]
    public static partial void BookDeleteFailed(ILogger logger, int id, string message);

    [LoggerMessage(21, LogLevel.Debug, "Update book {book}")]
    public static partial void BookUpdate(ILogger logger, Book book);

    [LoggerMessage(22, LogLevel.Error, "Failed to update {book}, error {message}.")]
    public static partial void BookUpdateFailed(ILogger logger, Book book, string message);

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
}
