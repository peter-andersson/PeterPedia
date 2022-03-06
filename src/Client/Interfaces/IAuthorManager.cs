
namespace PeterPedia.Client.Interfaces
{
    public interface IAuthorManager
    {
        Task<List<Author>> GetAsync();

        Task<bool> AddAsync(Author author);

        Task<bool> UpdateAsync(Author author);

        Task<bool> DeleteAsync(int id);

        Task RefreshAsync();

        public event Action? AuthorChanged;
    }
}
