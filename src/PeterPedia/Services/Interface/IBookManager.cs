namespace PeterPedia.Services;

public interface IBookManager
{
    Task<Result<Book>> AddAsync(Book book);

    Task<Result<Book>> DeleteAsync(int id);

    Task<Result<IList<Book>>> GetAllAsync();

    Task<Result<Book>> GetAsync(int id);

    Task<Result<Book>> UpdateAsync(Book book);
}
