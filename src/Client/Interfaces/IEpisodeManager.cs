
namespace PeterPedia.Client.Interfaces;

public interface IEpisodeManager
{
    event Action? EpisodeChanged;

    Task<bool> AddAsync(string url);
    Task<bool> DeleteAsync(int id);
    Task<List<Show>> GetAsync();
    Task<List<Episode>> GetEpisodesAsync();
    Task RefreshAsync();
    Task<bool> UpdateAsync(Show show);
    Task<bool> WatchAsync(int showId, ShowWatchData data);
}
