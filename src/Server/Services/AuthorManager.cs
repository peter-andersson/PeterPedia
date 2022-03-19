using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Server.Services;

public record AuthorResult(bool Success, string ErrorMessage, Author? Author);

public interface IAuthorManager
{
    Task<AuthorResult> AddAsync(Author author);

    Task<AuthorResult> DeleteAsync(int id);

    Task<IList<Author>> GetAsync(DateTime updateSince);

    Task<IList<DeleteLog>> GetDeletedAsync(DateTime deletedSince);

    Task<AuthorResult> UpdateAsync(Author author);
}

public class AuthorManager : IAuthorManager
{
    private readonly PeterPediaContext _dbContext;
    private readonly IDeleteTracker _deleteTracker;

    public AuthorManager(PeterPediaContext dbContext, IDeleteTracker deleteTracker)
    {
        _dbContext = dbContext;
        _deleteTracker = deleteTracker;
    }

    public async Task<AuthorResult> AddAsync(Author author)
    {
        AuthorEF? existingAuthor = await _dbContext.Authors.Where(a => a.Name == author.Name.Trim() && a.DateOfBirth == author.DateOfBirth).FirstOrDefaultAsync().ConfigureAwait(false);
        if (existingAuthor != null)
        {
            return new AuthorResult(false, "The author already exists.", null);
        }

        var authorEF = new AuthorEF
        {
            Name = author.Name.Trim(),
            DateOfBirth = author.DateOfBirth,
            LastUpdated = DateTime.UtcNow,
        };

        _dbContext.Authors.Add(authorEF);
        await _dbContext.SaveChangesAsync();

        return new AuthorResult(true, string.Empty, ConvertToAuthor(authorEF));
    }

    public async Task<AuthorResult> DeleteAsync(int id)
    {
        AuthorEF? author = await _dbContext.Authors.Where(a => a.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

        if (author is null)
        {
            return new AuthorResult(false, "Author with id doesn't exists", null);
        }

        _dbContext.Authors.Remove(author);
        await _dbContext.SaveChangesAsync();
        await _deleteTracker.TrackAsync(DeleteType.Author, id);

        return new AuthorResult(true, string.Empty, null);
    }

    public async Task<IList<Author>> GetAsync(DateTime updateSince)
    {
        List<AuthorEF>? authors = await _dbContext.Authors
            .Where(a => a.LastUpdated > updateSince || a.LastUpdated == DateTime.MinValue)
            .ToListAsync();

        var result = new List<Author>(authors.Count);
        foreach (AuthorEF? author in authors)
        {
            result.Add(ConvertToAuthor(author));
        }

        return result;
    }

    public async Task<IList<DeleteLog>> GetDeletedAsync(DateTime deletedSince) => await _deleteTracker.DeletedSinceAsync(DeleteType.Author, deletedSince);

    public async Task<AuthorResult> UpdateAsync(Author author)
    {
        AuthorEF? existingAuthor = await _dbContext.Authors
            .Where(a => a.Id == author.Id)
            .AsTracking()
            .SingleOrDefaultAsync();

        if (existingAuthor is null)
        {
            return new AuthorResult(false, "Author doesn't exists, can update it.", null);
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

        return new AuthorResult(true, string.Empty, ConvertToAuthor(existingAuthor));
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
