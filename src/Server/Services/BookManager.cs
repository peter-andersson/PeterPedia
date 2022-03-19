using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Server.Services;

public record BookResult(bool Success, string ErrorMessage, Book? Book);

public interface IBookManager
{
    Task<BookResult> AddAsync(Book book);

    Task<BookResult> DeleteAsync(int id);

    Task<IList<Book>> GetAsync(DateTime updateSince);

    Task<IList<DeleteLog>> GetDeletedAsync(DateTime deletedSince);

    Task<BookResult> UpdateAsync(Book book);
}

public class BookManager : IBookManager
{
    private readonly PeterPediaContext _dbContext;
    private readonly IDeleteTracker _deleteTracker;
    private readonly IFileService _fileService;
    private readonly IConfiguration _configuration;

    public BookManager(
        PeterPediaContext dbContext,
        IDeleteTracker deleteTracker,
        IFileService fileService,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _deleteTracker = deleteTracker;
        _fileService = fileService;
        _configuration = configuration;
    }

    public async Task<BookResult> AddAsync(Book book)
    {
        var bookEF = new BookEF
        {
            Title = book.Title,
            State = (int)book.State,
            LastUpdated = DateTime.UtcNow,
            Authors = new List<AuthorEF>(),
        };

        foreach (Author author in book.Authors)
        {
            AuthorEF? authorEF = await _dbContext.Authors.Where(a => a.Name == author.Name.Trim() && a.DateOfBirth == author.DateOfBirth).AsTracking().FirstOrDefaultAsync();
            if (authorEF is not null)
            {
                bookEF.Authors.Add(authorEF);
            }
        }

        _dbContext.Books.Add(bookEF);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);

        await DownloadCoverAsync(book.CoverUrl, bookEF.Id);

        return new BookResult(true, string.Empty, ConvertToBook(bookEF));
    }

    public async Task<BookResult> DeleteAsync(int id)
    {
        BookEF? book = await _dbContext.Books.Where(b => b.Id == id).AsTracking().SingleOrDefaultAsync();

        if (book is null)
        {
            return new BookResult(false, "Book with id doesn't exists", null);
        }

        _dbContext.Books.Remove(book);
        await _dbContext.SaveChangesAsync();
        await _deleteTracker.TrackAsync(DeleteType.Book, id);

        return new BookResult(true, string.Empty, null);
    }

    public async Task<IList<Book>> GetAsync(DateTime updateSince)
    {
        List<BookEF>? books = await _dbContext.Books
            .Include(b => b.Authors)
            .AsSplitQuery()
            .Where(b => b.LastUpdated > updateSince || b.LastUpdated == DateTime.MinValue)
            .ToListAsync()
            .ConfigureAwait(false);

        var result = new List<Book>(books.Count);

        foreach (BookEF? book in books)
        {
            result.Add(ConvertToBook(book));
        }

        return result;
    }

    public async Task<IList<DeleteLog>> GetDeletedAsync(DateTime deletedSince) => await _deleteTracker.DeletedSinceAsync(DeleteType.Book, deletedSince);

    public async Task<BookResult> UpdateAsync(Book book)
    {
        BookEF? bookEF = await _dbContext.Books
            .Where(b => b.Id == book.Id)
            .Include(b => b.Authors)
            .AsSplitQuery()
            .AsTracking()
            .SingleOrDefaultAsync();

        if (bookEF is null)
        {
            return new BookResult(false, "Book doesn't exist", null);
        }

        bookEF.Authors.Clear();
        foreach (Author author in book.Authors)
        {
            AuthorEF? authorEF = await _dbContext.Authors.Where(a => a.Name == author.Name.Trim() && (a.DateOfBirth == null || (a.DateOfBirth == author.DateOfBirth))).AsTracking().FirstOrDefaultAsync();
            if (authorEF is not null)
            {
                bookEF.Authors.Add(authorEF);
            }
        }

        bookEF.State = (int)book.State;
        bookEF.Title = book.Title;
        bookEF.LastUpdated = DateTime.UtcNow;

        _dbContext.Books.Update(bookEF);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);

        await DownloadCoverAsync(book.CoverUrl, bookEF.Id);

        return new BookResult(true, string.Empty, ConvertToBook(bookEF));
    }

    private static Book ConvertToBook(BookEF bookEF)
    {
        if (bookEF is null)
        {
            throw new ArgumentNullException(nameof(bookEF));
        }

        var book = new Book()
        {
            Id = bookEF.Id,
            Title = bookEF.Title,
            LastUpdated = bookEF.LastUpdated,
            State = (BookState)bookEF.State,
        };

        foreach (AuthorEF? author in bookEF.Authors)
        {
            book.Authors.Add(ConvertToAuthor(author));
        }

        return book;
    }

    private static Author ConvertToAuthor(AuthorEF authorEF)
    {
        if (authorEF is null)
        {
            throw new ArgumentNullException(nameof(authorEF));
        }

        var author = new Author()
        {
            Id = authorEF.Id,
            Name = authorEF.Name,
            DateOfBirth = authorEF.DateOfBirth ?? DateOnly.MinValue,
            LastUpdated = authorEF.LastUpdated,
        };

        return author;
    }

    private async Task DownloadCoverAsync(string? url, int id)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        var filename = Path.Combine(_configuration["ImagePath"], "books", $"{id}.jpg");
        await _fileService.DownloadImageAsync(url, filename);
    }
}
