
namespace PeterPedia.Client.Interfaces;

public interface IMovieManager
{
    event Action? MovieChanged;

    Task<bool> AddAsync(string movieUrl);

    Task<bool> DeleteAsync(int id);

    Task<List<Movie>> GetAsync();

    Task RefreshAsync();

    Task<bool> UpdateAsync(Movie movie);
}
