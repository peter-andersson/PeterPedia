using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Server.Services;

public interface IAuthorManager
{
    Task<Result<Author>> AddAsync(Author author);

    Task<Result<Author>> DeleteAsync(int id);

    Task<Result<IList<Author>>> GetAsync(DateTime updateSince);

    Task<Result<IList<DeleteLog>>> GetDeletedAsync(DateTime deletedSince);

    Task<Result<Author>> UpdateAsync(Author author);
}

public class AuthorManager : IAuthorManager
{
    private readonly PeterPediaContext _dbContext;
    private readonly IDeleteTracker _deleteTracker;
    private readonly ILogger<AuthorManager> _logger;

    public AuthorManager(PeterPediaContext dbContext, IDeleteTracker deleteTracker, ILogger<AuthorManager> logger)
    {
        _dbContext = dbContext;
        _deleteTracker = deleteTracker;
        _logger = logger;
    }

    public async Task<Result<Author>> AddAsync(Author author)
    {
        LogMessage.AuthorAdd(_logger, author);

        AuthorEF? existingAuthor = await _dbContext.Authors.Where(a => a.Name == author.Name.Trim() && a.DateOfBirth == author.DateOfBirth).FirstOrDefaultAsync().ConfigureAwait(false);
        if (existingAuthor != null)
        {
            LogMessage.AuthorAddFailed(_logger, author, "The author already exists.");
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

        return new SuccessResult<Author>(ConvertToAuthor(authorEF));
    }

    public async Task<Result<Author>> DeleteAsync(int id)
    {
        LogMessage.AuthorDelete(_logger, id);

        AuthorEF? author = await _dbContext.Authors.Where(a => a.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

        if (author is null)
        {
            LogMessage.AuthorDeleteFailed(_logger, id, "Author with id doesn't exists");
            return new NotFoundResult<Author>();
        }

        _dbContext.Authors.Remove(author);
        await _dbContext.SaveChangesAsync();
        await _deleteTracker.TrackAsync(DeleteType.Author, id);

        return new SuccessResult<Author>(ConvertToAuthor(author));
    }

    public async Task<Result<IList<Author>>> GetAsync(DateTime updateSince)
    {
        List<AuthorEF>? authors = await _dbContext.Authors
            .Where(a => a.LastUpdated > updateSince || a.LastUpdated == DateTime.MinValue)
            .ToListAsync();

        var result = new List<Author>(authors.Count);
        foreach (AuthorEF? author in authors)
        {
            result.Add(ConvertToAuthor(author));
        }

        return new SuccessResult<IList<Author>>(result);
    }

    public async Task<Result<IList<DeleteLog>>> GetDeletedAsync(DateTime deletedSince) =>
        new SuccessResult<IList<DeleteLog>>(await _deleteTracker.DeletedSinceAsync(DeleteType.Author, deletedSince));

    public async Task<Result<Author>> UpdateAsync(Author author)
    {
        LogMessage.AuthorUpdate(_logger, author);

        AuthorEF? existingAuthor = await _dbContext.Authors
            .Where(a => a.Id == author.Id)
            .AsTracking()
            .SingleOrDefaultAsync();

        if (existingAuthor is null)
        {
            LogMessage.AuthorUpdateFailed(_logger, author, "Author doesn't exists, can update it.");
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
        }

        return new SuccessResult<Author>(ConvertToAuthor(existingAuthor));
    }

    private static Author ConvertToAuthor(AuthorEF authorEF)
    {
        return new Author()
        {
            Id = authorEF.Id,
            Name = authorEF.Name,
            DateOfBirth = authorEF.DateOfBirth ?? DateOnly.MinValue,
            LastUpdated = authorEF.LastUpdated,
        };
    }    
}
