using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class BookController : Controller
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used by source generator [LoggerMessaage]")]
    private readonly ILogger<BookController> _logger;

    private readonly PeterPediaContext _dbContext;

    public BookController(ILogger<BookController> logger, PeterPediaContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet]
    [Route("{*lastUpdated:datetime}")]
    public async Task<IActionResult> GetAsync(DateTime lastUpdated)
    {
        List<BookEF>? books = await _dbContext.Books
            .Include(b => b.Authors)
            .AsSplitQuery()
            .Where(b => b.LastUpdated.GetValueOrDefault(DateTime.MaxValue) > lastUpdated)
            .ToListAsync()
            .ConfigureAwait(false);

        var result = new List<Book>(books.Count);
        foreach (BookEF? book in books)
        {
            result.Add(ConvertToBook(book));
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Book book)
    {
        if (book is null)
        {
            return BadRequest();
        }

        LogAddBook(book);

        var bookEF = new BookEF
        {
            Title = book.Title,
            State = (int)book.State,
            LastUpdated = DateTime.UtcNow,
            Authors = new List<AuthorEF>(),
        };

        foreach (var name in book.Authors)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            AuthorEF? author = await _dbContext.Authors.Where(a => a.Name == name.Trim()).AsTracking().FirstOrDefaultAsync();

            if (author is null)
            {
                author = new AuthorEF()
                {
                    Name = name.Trim(),
                };

                LogAddAuthor(name);

                _dbContext.Authors.Add(author);
            }

            bookEF.Authors.Add(author);
        }

        _dbContext.Books.Add(bookEF);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);

        LogBookAdded(bookEF);

        return Ok(ConvertToBook(bookEF));
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync([FromBody] Book book)
    {
        if (book is null)
        {
            return BadRequest();
        }

        LogUpdateBook(book);

        BookEF? bookEF = await _dbContext.Books.Where(b => b.Id == book.Id).Include(b => b.Authors).AsSplitQuery().AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);
        if (bookEF is null)
        {
            LogNotFound(book.Id);
            return NotFound();
        }

        bookEF.Authors.Clear();
        foreach (var name in book.Authors)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            AuthorEF? author = await _dbContext.Authors.Where(a => a.Name == name.Trim()).AsTracking().FirstOrDefaultAsync();

            if (author is null)
            {
                author = new AuthorEF()
                {
                    Name = name.Trim(),
                };

                LogAddAuthor(name);
                _dbContext.Authors.Add(author);
            }

            bookEF.Authors.Add(author);
        }

        bookEF.State = (int)book.State;
        bookEF.Title = book.Title;
        bookEF.LastUpdated = DateTime.UtcNow;

        _dbContext.Books.Update(bookEF);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        LogBookUpdated(bookEF);

        return Ok(ConvertToBook(bookEF));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        LogDeleteBook(id);
        if (id <= 0)
        {
            return BadRequest();
        }

        BookEF? bookEF = await _dbContext.Books.Where(b => b.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

        if (bookEF is null)
        {
            LogNotFound(id);
            return NotFound();
        }

        _dbContext.Books.Remove(bookEF);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);

        LogBookDeleted(bookEF);

        return Ok();
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
            book.Authors.Add(author.Name);
        }

        return book;
    }

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter
    [LoggerMessage(0, LogLevel.Debug, "Adding new book {book}")]
    partial void LogAddBook(Book book);

    [LoggerMessage(1, LogLevel.Debug, "Adding new author {name}.")]
    partial void LogAddAuthor(string name);

    [LoggerMessage(2, LogLevel.Debug, "Book {book} added.")]
    partial void LogBookAdded(BookEF book);

    [LoggerMessage(3, LogLevel.Debug, "Update book {book}")]
    partial void LogUpdateBook(Book book);

    [LoggerMessage(4, LogLevel.Debug, "Book {book} updated.")]
    partial void LogBookUpdated(BookEF book);

    [LoggerMessage(5, LogLevel.Debug, "Book with id {id} not found.")]
    partial void LogNotFound(int id);

    [LoggerMessage(6, LogLevel.Debug, "Delete book with id {id}.")]
    partial void LogDeleteBook(int id);

    [LoggerMessage(7, LogLevel.Debug, "Delete book {book}.")]
    partial void LogBookDeleted(BookEF book);
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression
}
