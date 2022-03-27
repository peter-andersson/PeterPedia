namespace PeterPedia.Server.Helpers;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used by source generator")]
public static partial class LogMessage
{    
    [LoggerMessage(0, LogLevel.Error, "Failed to fetch data from themoviedb.org")]
    public static partial void TheMovieDbFailed(ILogger logger);

    [LoggerMessage(1, LogLevel.Debug, "Add movie {id}")]
    public static partial void AddMovie(ILogger logger, int id);

    [LoggerMessage(2, LogLevel.Error, "Failed to add movie {movie}, error {message}")]
    public static partial void MovieAddFailed(ILogger logger, AddMovie movie, string message);

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

    [LoggerMessage(18, LogLevel.Error, "Failed to add book {book}, error = {message}.")]
    public static partial void BookAddFailed(ILogger logger, Book book, string message);

    [LoggerMessage(19, LogLevel.Debug, "Delete book with id {id}.")]
    public static partial void BookDelete(ILogger logger, int id);

    [LoggerMessage(20, LogLevel.Error, "Failed ot delete book with id {id}, error = {message}")]
    public static partial void BookDeleteFailed(ILogger logger, int id, string message);

    [LoggerMessage(21, LogLevel.Debug, "Update book {book}")]
    public static partial void BookUpdate(ILogger logger, Book book);

    [LoggerMessage(22, LogLevel.Error, "Failed to update {book}, error {message}.")]
    public static partial void BookUpdateFailed(ILogger logger, Book book, string message);

    [LoggerMessage(23, LogLevel.Debug, "Downloading image from {url} to {filename}")]
    public static partial void DownloadImage(ILogger logger, string url, string filename);

    [LoggerMessage(24, LogLevel.Error, "Failed to download image from {url} to {filename}.")]
    public static partial void DownloadImageFailed(ILogger logger, string url, string filename, Exception e);
}
