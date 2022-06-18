namespace PeterPedia.Services;

public class Library : ILibrary
{
    private readonly ILogger<Library> _logger;
    private readonly PeterPediaContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public Library(
        ILogger<Library> logger,
        PeterPediaContext dbContext,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _dbContext = dbContext;        
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<Result<Author>> AddAuthorAsync(Author author)
    {
        LibraryMessages.AuthorAdd(_logger, author.Name);

        AuthorEF? existingAuthor = await _dbContext.Authors.Where(a => a.Name == author.Name.Trim() && a.DateOfBirth == author.DateOfBirth).FirstOrDefaultAsync().ConfigureAwait(false);
        if (existingAuthor != null)
        {
            LibraryMessages.AuthorAddFailed(_logger, author.Name, "The author already exists.");
            return new ConflictResult<Author>();
        }

        var authorEF = new AuthorEF
        {
            Name = author.Name.Trim(),
            DateOfBirth = author.DateOfBirth,
            LastUpdated = DateTime.UtcNow,
        };

        _dbContext.Authors.Add(authorEF);
        await _dbContext.SaveChangesAsync();

        LibraryMessages.AuthorAdded(_logger, authorEF.Id);

        return new SuccessResult<Author>(ConvertToAuthor(authorEF));
    }

    public async Task<Result<Book>> AddBookAsync(Book book)
    {
        LibraryMessages.BookAdd(_logger, book.Title, book.AuthorText);

        var bookEF = new BookEF
        {
            Title = book.Title,
            State = (int)book.State,
            LastUpdated = DateTime.UtcNow,
            Authors = new List<AuthorEF>(),
        };

        foreach (Author author in book.Authors)
        {
            AuthorEF? authorEF = await _dbContext.Authors.Where(a => a.Id == author.Id).AsTracking().FirstOrDefaultAsync();
            if (authorEF is not null)
            {
                bookEF.Authors.Add(authorEF);
            }
        }

        _dbContext.Books.Add(bookEF);
        await _dbContext.SaveChangesAsync();

        LibraryMessages.BookAdded(_logger, bookEF.Id);

        await DownloadCoverAsync(book.CoverUrl, bookEF.Id);

        return new SuccessResult<Book>(ConvertToBook(bookEF));
    }

    public async Task<Result<Author>> DeleteAuthorAsync(int id)
    {
        LibraryMessages.AuthorDelete(_logger, id);

        AuthorEF? author = await _dbContext.Authors.Where(a => a.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

        if (author is null)
        {
            LibraryMessages.AuthorDeleteFailed(_logger, id, "Author with id doesn't exists");
            return new NotFoundResult<Author>();
        }

        _dbContext.Authors.Remove(author);
        await _dbContext.SaveChangesAsync();

        LibraryMessages.AuthorDeleted(_logger, id);

        return new SuccessResult<Author>(ConvertToAuthor(author));
    }

    public async Task<Result<Book>> DeleteBookAsync(int id)
    {
        LibraryMessages.BookDelete(_logger, id);

        BookEF? book = await _dbContext.Books.Where(b => b.Id == id).AsTracking().SingleOrDefaultAsync();

        if (book is null)
        {
            LibraryMessages.BookDeleteFailed(_logger, id, "No book found.");
            return new NotFoundResult<Book>();
        }

        _dbContext.Books.Remove(book);
        await _dbContext.SaveChangesAsync();

        LibraryMessages.BookDeleted(_logger, id);

        return new SuccessResult<Book>(ConvertToBook(book));
    }

    public async Task<Result<IList<Author>>> GetAuthorsAsync()
    {
        List<AuthorEF>? authors = await _dbContext.Authors
            .OrderBy(a => a.Name)
            .ToListAsync();

        var result = new List<Author>(authors.Count);
        foreach (AuthorEF? author in authors)
        {
            result.Add(ConvertToAuthor(author));
        }

        return new SuccessResult<IList<Author>>(result);
    }

    public async Task<Result<Author>> GetAuthorAsync(int id)
    {
        AuthorEF? authorEF = await _dbContext.Authors
            .Where(a => a.Id == id)
            .SingleOrDefaultAsync();

        return authorEF is null
            ? new ErrorResult<Author>("No author found.")
            : new SuccessResult<Author>(ConvertToAuthor(authorEF));
    }

    public async Task<Result<IList<Book>>> GetBooksAsync()
    {
        List<BookEF>? books = await _dbContext.Books
            .Include(b => b.Authors)
            .OrderBy(b => b.Title)
            .AsSplitQuery()
            .ToListAsync();

        var result = new List<Book>(books.Count);

        foreach (BookEF? book in books)
        {
            result.Add(ConvertToBook(book));
        }

        return new SuccessResult<IList<Book>>(result);
    }

    public async Task<Result<Book>> GetBookAsync(int id)
    {
        BookEF? bookEF = await _dbContext.Books
            .Include(b => b.Authors)
            .Where(b => b.Id == id)
            .AsSplitQuery()
            .SingleOrDefaultAsync();

        return bookEF is not null
            ? new SuccessResult<Book>(ConvertToBook(bookEF))
            : new ErrorResult<Book>("No book found");
    }

    public async Task<Result<Author>> UpdateAuthorAsync(Author author)
    {
        LibraryMessages.AuthorUpdate(_logger, author.Id, author.Name);

        AuthorEF? existingAuthor = await _dbContext.Authors
            .Where(a => a.Id == author.Id)
            .AsTracking()
            .SingleOrDefaultAsync();

        if (existingAuthor is null)
        {
            LibraryMessages.AuthorUpdateFailed(_logger, author.Id, "Author doesn't exists, can update it.");
            return new NotFoundResult<Author>();
        }

        if ((author.Name.Trim() != existingAuthor.Name) ||
            (author.DateOfBirth != existingAuthor.DateOfBirth))
        {
            existingAuthor.Name = author.Name.Trim();
            existingAuthor.DateOfBirth = author.DateOfBirth;
            existingAuthor.LastUpdated = DateTime.UtcNow;

            _dbContext.Authors.Update(existingAuthor);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            LibraryMessages.AuthorUpdated(_logger, existingAuthor.Id);
        }
        
        return new SuccessResult<Author>(ConvertToAuthor(existingAuthor));
    }

    public async Task<Result<Book>> UpdateBookAsync(Book book)
    {
        LibraryMessages.BookUpdate(_logger, book.Id, book.Title, book.AuthorText);

        BookEF? bookEF = await _dbContext.Books
            .Where(b => b.Id == book.Id)
            .Include(b => b.Authors)
            .AsSplitQuery()
            .AsTracking()
            .SingleOrDefaultAsync();

        if (bookEF is null)
        {
            LibraryMessages.BookUpdateFailed(_logger, book.Id, "Book not found.");
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

        LibraryMessages.BookUpdated(_logger, bookEF.Id);

        await DownloadCoverAsync(book.CoverUrl, bookEF.Id);

        return new SuccessResult<Book>(ConvertToBook(bookEF));
    }

    private async Task DownloadCoverAsync(string? url, int id)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        var filename = Path.Combine(_configuration["ImagePath"], "books", $"{id}.jpg");

        if (File.Exists(filename))
        {
            File.Delete(filename);
        }

        var dir = Path.GetDirectoryName(filename);
        if (string.IsNullOrEmpty(dir))
        {
            return;
        }

        Directory.CreateDirectory(dir);

        LibraryMessages.DownloadImage(_logger, url, filename);

        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var fs = new FileStream(filename, FileMode.CreateNew);

            await response.Content.CopyToAsync(fs);
        }
        catch (Exception ex)
        {
            LibraryMessages.DownloadImageFailed(_logger, url, filename, ex);
        }
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

        if (bookEF.Authors is not null)
        {
            foreach (AuthorEF? author in bookEF.Authors)
            {
                book.Authors.Add(ConvertToAuthor(author));
            }
        }

        return book;
    }
       
    private static Author ConvertToAuthor(AuthorEF authorEF)
    {
        return new Author()
        {
            Id = authorEF.Id,
            Name = authorEF.Name,
            DateOfBirth = authorEF.DateOfBirth ?? DateOnly.MinValue
        };
    }    
}
