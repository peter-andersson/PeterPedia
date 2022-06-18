namespace PeterPedia.Services;

public interface ILibrary
{
    Task<Result<Author>> AddAuthorAsync(Author author);

    Task<Result<Book>> AddBookAsync(Book book);

    Task<Result<Author>> DeleteAuthorAsync(int id);

    Task<Result<Book>> DeleteBookAsync(int id);

    Task<Result<IList<Author>>> GetAuthorsAsync();

    Task<Result<Author>> GetAuthorAsync(int id);

    Task<Result<IList<Book>>> GetBooksAsync();

    Task<Result<Book>> GetBookAsync(int id);

    Task<Result<Author>> UpdateAuthorAsync(Author author);

    Task<Result<Book>> UpdateBookAsync(Book book);    
}
