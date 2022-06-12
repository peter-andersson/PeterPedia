namespace PeterPedia.Services;

public interface IAuthorManager
{
    Task<Result<Author>> AddAsync(Author author);

    Task<Result<Author>> DeleteAsync(int id);

    Task<Result<IList<Author>>> GetAllAsync();

    Task<Result<Author>> GetAsync(int id);

    Task<Result<Author>> UpdateAsync(Author author);
}
