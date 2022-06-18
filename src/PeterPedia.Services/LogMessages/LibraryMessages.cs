namespace PeterPedia.Services.LogMessages;

public static partial class LibraryMessages
{
    [LoggerMessage(1, LogLevel.Debug, "Adding book, {title} by {authors}.")]
    public static partial void BookAdd(ILogger logger, string title, string authors);

    [LoggerMessage(2, LogLevel.Debug, "Added book with id {id}.")]
    public static partial void BookAdded(ILogger logger, int id);

    [LoggerMessage(3, LogLevel.Debug, "Delete book with id {id}.")]
    public static partial void BookDelete(ILogger logger, int id);

    [LoggerMessage(4, LogLevel.Debug, "Deleted book with id {id}.")]
    public static partial void BookDeleted(ILogger logger, int id);

    [LoggerMessage(5, LogLevel.Error, "Failed to delete book with id {id}, error \"{message}\".")]
    public static partial void BookDeleteFailed(ILogger logger, int id, string message);

    [LoggerMessage(6, LogLevel.Debug, "Update book {id}, {title} by {authors}.")]
    public static partial void BookUpdate(ILogger logger, int id, string title, string authors);

    [LoggerMessage(7, LogLevel.Debug, "Updated book {id}.")]
    public static partial void BookUpdated(ILogger logger, int id);

    [LoggerMessage(8, LogLevel.Error, "Failed to update book with id {id}, error \"{message}\".")]
    public static partial void BookUpdateFailed(ILogger logger, int id, string message);

    [LoggerMessage(9, LogLevel.Debug, "Adding author {name}.")]
    public static partial void AuthorAdd(ILogger logger, string name);

    [LoggerMessage(10, LogLevel.Debug, "Added author with id {id}.")]
    public static partial void AuthorAdded(ILogger logger, int id);

    [LoggerMessage(11, LogLevel.Error, "Failed to add author {name}, error \"{message}\".")]
    public static partial void AuthorAddFailed(ILogger logger, string name, string message);

    [LoggerMessage(12, LogLevel.Debug, "Delete author with id {id}.")]
    public static partial void AuthorDelete(ILogger logger, int id);

    [LoggerMessage(13, LogLevel.Debug, "Deletde author with id {id}.")]
    public static partial void AuthorDeleted(ILogger logger, int id);

    [LoggerMessage(14, LogLevel.Error, "Failed to delete author with id {id}, error \"{message}\".")]
    public static partial void AuthorDeleteFailed(ILogger logger, int id, string message);

    [LoggerMessage(15, LogLevel.Debug, "Update author {id}, {name}.")]
    public static partial void AuthorUpdate(ILogger logger, int id, string name);

    [LoggerMessage(16, LogLevel.Debug, "Updated author {id}.")]
    public static partial void AuthorUpdated(ILogger logger, int id);

    [LoggerMessage(17, LogLevel.Error, "Failed to update author with id {id}, error \"{message}\".")]
    public static partial void AuthorUpdateFailed(ILogger logger, int id, string message);

    [LoggerMessage(18, LogLevel.Debug, "Downloading image from {url} to {filename}")]
    public static partial void DownloadImage(ILogger logger, string url, string filename);

    [LoggerMessage(19, LogLevel.Error, "Failed to download image from {url} to {filename}.")]
    public static partial void DownloadImageFailed(ILogger logger, string url, string filename, Exception e);
}
