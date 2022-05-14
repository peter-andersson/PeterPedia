namespace PeterPedia.Services;

public interface ITheMovieDatabaseService
{
    Task<string> GetImageUrlAsync(string? path, string size = "w185");

    Task<TMDbMovie?> GetMovieAsync(int id, string? etag);

    Task<TMDbShow?> GetTvShowAsync(int id, string? etag);
}
