
namespace PeterPedia.Client.Interfaces;

public interface IBookManager
{
    Task<List<Book>> GetAsync();

    Task<bool> AddAsync(Book book);

    Task<bool> UpdateAsync(Book book);

    Task<bool> DeleteAsync(int id);

    Task RefreshAsync();

    public event Action? BookChanged;
}
