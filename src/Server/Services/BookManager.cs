using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Server.Services;

public interface IBookManager
{
    Task<Result<Book>> AddAsync(Book book);

    Task<Result<Book>> DeleteAsync(int id);

    Task<Result<IList<Book>>> GetAsync(DateTime updateSince);

    Task<Result<IList<DeleteLog>>> GetDeletedAsync(DateTime deletedSince);

    Task<Result<Book>> UpdateAsync(Book book);
}

public class BookManager : IBookManager
{
    private readonly PeterPediaContext _dbContext;
    private readonly IDeleteTracker _deleteTracker;
    private readonly IFileService _fileService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BookManager> _logger;

    public BookManager(
        PeterPediaContext dbContext,
        IDeleteTracker deleteTracker,
        IFileService fileService,
        IConfiguration configuration,
        ILogger<BookManager> logger)
    {
        _dbContext = dbContext;
        _deleteTracker = deleteTracker;
        _fileService = fileService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<Book>> AddAsync(Book book)
    {
        LogMessage.BookAdd(_logger, book);

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

        return new SuccessResult<Book>(ConvertToBook(bookEF));
    }

    public async Task<Result<Book>> DeleteAsync(int id)
    {
        LogMessage.BookDelete(_logger, id);

        BookEF? book = await _dbContext.Books.Where(b => b.Id == id).AsTracking().SingleOrDefaultAsync();

        if (book is null)
        {
            LogMessage.BookDeleteFailed(_logger, id, "No book found.");
            return new NotFoundResult<Book>();
        }

        _dbContext.Books.Remove(book);
        await _dbContext.SaveChangesAsync();
        await _deleteTracker.TrackAsync(DeleteType.Book, id);

        return new SuccessResult<Book>(ConvertToBook(book));
    }

    public async Task<Result<IList<Book>>> GetAsync(DateTime updateSince)
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

        return new SuccessResult<IList<Book>>(result);
    }

    public async Task<Result<IList<DeleteLog>>> GetDeletedAsync(DateTime deletedSince) =>
        new SuccessResult<IList<DeleteLog>>(await _deleteTracker.DeletedSinceAsync(DeleteType.Book, deletedSince));

    public async Task<Result<Book>> UpdateAsync(Book book)
    {
        LogMessage.BookUpdate(_logger, book);

        BookEF? bookEF = await _dbContext.Books
            .Where(b => b.Id == book.Id)
            .Include(b => b.Authors)
            .AsSplitQuery()
            .AsTracking()
            .SingleOrDefaultAsync();

        if (bookEF is null)
        {
            LogMessage.BookUpdateFailed(_logger, book, "Book not found.");
            return new NotFoundResult<Book>();
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

        return new SuccessResult<Book>(ConvertToBook(bookEF));
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
