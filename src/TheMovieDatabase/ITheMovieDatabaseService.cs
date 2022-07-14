namespace TheMovieDatabase;

public interface ITheMovieDatabaseService
{
    Task<string> GetImageUrlAsync(string? path, string size = "w185");

    Task<TMDbMovie?> GetMovieAsync(string id, string? etag);

    Task<TMDbShow?> GetTvShowAsync(int id, string? etag);

    Task DownloadImageUrlToStreamAsync(string url, Stream stream);
}
