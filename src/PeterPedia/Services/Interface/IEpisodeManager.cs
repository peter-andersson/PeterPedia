namespace PeterPedia.Services;

public interface IEpisodeManager
{
    Task<Result<string>> AddAsync(int showId);

    Task<Result> DeleteAsync(int id);

    Task<Result<IList<Show>>> GetAllAsync();

    Task<Result<IList<Show>>> GetWatchlistAsync();

    Task<Result<Show>> GetAsync(int id);

    Task<Result<IList<Episode>>> GetEpisodesAsync();

    Task<Result<Show>> UpdateAsync(Show show);

    Task<Result> WatchAsync(ShowWatchData watchData);

    Task RefreshAsync();
}
