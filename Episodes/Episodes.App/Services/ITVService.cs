namespace Episodes.App.Services;

public interface ITVService
{
    event Action? OnChange;

    Task<Result> AddAsync(int? id);

    Task<Result> DeleteAsync(TVShow show);

    Task<TVShow?> GetAsync(string id);

    Task<TVShow[]> GetAsync(QueryData query);

    Task<TVShow[]> GetWatchListAsync();

    Task<Result> UpdateAsync(TVShow show);
}
