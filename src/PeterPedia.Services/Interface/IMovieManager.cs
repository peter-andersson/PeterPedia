namespace PeterPedia.Services;

public interface IMovieManager
{
    Task<Result<string>> AddAsync(int id);

    Task<Result> DeleteAsync(int id);

    Task<IList<Movie>> GetAllAsync();

    Task<IList<Movie>> GetWatchListAsync();

    Task<Result<Movie>> GetAsync(int id);

    Task<Result> UpdateAsync(Movie movie);

    Task RefreshAsync();
}
