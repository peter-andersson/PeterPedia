using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class AuthorController : Controller
{    
    private readonly ILogger<AuthorController> _logger;

    private readonly PeterPediaContext _dbContext;

    public AuthorController(ILogger<AuthorController> logger, PeterPediaContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery]string lastupdate)
    {
        if (!DateTime.TryParseExact(lastupdate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastUpdated))
        {
            return BadRequest("Invalid date format");
        }        

        List<AuthorEF>? authors = await _dbContext.Authors
            .Where(a => a.LastUpdated > lastUpdated || a.LastUpdated == DateTime.MinValue)
            .ToListAsync()
            .ConfigureAwait(false);

        var result = new List<Author>(authors.Count);
        foreach (AuthorEF? author in authors)
        {
            result.Add(ConvertToAuthor(author));
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Author author)
    {
        if (author is null)
        {
            return BadRequest();
        }

        LogAddAuthor(author);

        AuthorEF? existingAuthor = await _dbContext.Authors.Where(s => s.Name == author.Name.Trim()).FirstOrDefaultAsync().ConfigureAwait(false);
        if (existingAuthor != null)
        {
            LogAuthorExists(existingAuthor);
            return Conflict();
        }

        var authorEF = new AuthorEF
        {
            Name = author.Name.Trim(),
            DateOfBirth = author.DateOfBirth,
            LastUpdated = DateTime.UtcNow,
        };

        _dbContext.Authors.Add(authorEF);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        LogAuthorAdded(authorEF);

        return Ok(ConvertToAuthor(authorEF));
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync([FromBody] Author author)
    {
        if (author is null)
        {
            return BadRequest();
        }

        LogUpdateAuthor(author);

        AuthorEF? existingAuthor = await _dbContext.Authors.Where(s => s.Id == author.Id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);
        if (existingAuthor is null)
        {
            LogNotFound(author.Id);
            return NotFound();
        }

        if ((author.Name.Trim() != existingAuthor.Name) ||
            (author.DateOfBirth != existingAuthor.DateOfBirth))
        {
            existingAuthor.Name = author.Name.Trim();
            existingAuthor.DateOfBirth = author.DateOfBirth;
            existingAuthor.LastUpdated = DateTime.UtcNow;

            _dbContext.Authors.Update(existingAuthor);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            LogAuthorUpdated(existingAuthor);
        }

        return Ok(ConvertToAuthor(existingAuthor));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        LogDeleteAuthor(id);
        if (id <= 0)
        {
            return BadRequest();
        }

        AuthorEF? author = await _dbContext.Authors.Where(a => a.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

        if (author is null)
        {
            LogNotFound(id);
            return NotFound();
        }

        _dbContext.Authors.Remove(author);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);

        LogAuthorDeleted(author);

        return NoContent();
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

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter
    [LoggerMessage(0, LogLevel.Debug, "Adding new author {author}")]
    partial void LogAddAuthor(Author author);

    [LoggerMessage(1, LogLevel.Debug, "Author {author} already exists.")]
    partial void LogAuthorExists(AuthorEF author);

    [LoggerMessage(2, LogLevel.Debug, "Author {author} added.")]
    partial void LogAuthorAdded(AuthorEF author);

    [LoggerMessage(3, LogLevel.Debug, "Update author {author}")]
    partial void LogUpdateAuthor(Author author);

    [LoggerMessage(4, LogLevel.Debug, "Author {author} updated.")]
    partial void LogAuthorUpdated(AuthorEF author);

    [LoggerMessage(5, LogLevel.Debug, "Author with id {id} not found.")]
    partial void LogNotFound(int id);

    [LoggerMessage(6, LogLevel.Debug, "Delete author with id {id}.")]
    partial void LogDeleteAuthor(int id);

    [LoggerMessage(7, LogLevel.Debug, "Delete author {author}.")]
    partial void LogAuthorDeleted(AuthorEF author);
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression
}
