using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeterPedia.Shared;
using PeterPedia.Server.Data;
using PeterPedia.Server.Data.Models;

namespace PeterPedia.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class AuthorController : Controller
{
#pragma warning disable IDE0052 // Remove unread private members
    private readonly ILogger<AuthorController> _logger;
#pragma warning restore IDE0052 // Remove unread private members
    private readonly PeterPediaContext _dbContext;

    public AuthorController(ILogger<AuthorController> logger, PeterPediaContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var authors = await _dbContext.Authors.ToListAsync().ConfigureAwait(false);

        var result = new List<Author>(authors.Count);
        foreach (var author in authors)
        {
            result.Add(ConvertToAuthor(author));
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Author author)
    {
        if (author is null)
        {
            return BadRequest();
        }

        LogAddAuthor(author);

        var existingAuthor = await _dbContext.Authors.Where(s => s.Name == author.Name.Trim()).FirstOrDefaultAsync().ConfigureAwait(false);
        if (existingAuthor != null)
        {
            LogAuthorExists(existingAuthor);
            return Conflict();
        }

        var authorEF = new AuthorEF
        {
            Name = author.Name.Trim(),
        };

        _dbContext.Authors.Add(authorEF);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        LogAuthorAdded(authorEF);

        return Ok(ConvertToAuthor(authorEF));
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Author author)
    {
        if (author is null)
        {
            return BadRequest();
        }

        LogUpdateAuthor(author);

        var existingAuthor = await _dbContext.Authors.Where(s => s.Id == author.Id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);
        if (existingAuthor is null)
        {
            LogNotFound(author.Id);
            return NotFound();
        }

        if (author.Name.Trim() != existingAuthor.Name)
        {
            existingAuthor.Name = author.Name.Trim();
            _dbContext.Authors.Update(existingAuthor);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            LogAuthorUpdated(existingAuthor);
        }

        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        LogDeleteAuthor(id);
        if (id <= 0)
        {
            return BadRequest();
        }

        var author = await _dbContext.Authors.Where(a => a.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

        if (author is null)
        {
            LogNotFound(id);
            return NotFound();
        }

        _dbContext.Authors.Remove(author);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);

        LogAuthorDeleted(author);

        return Ok();
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