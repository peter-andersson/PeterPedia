using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public partial class LinkController : ControllerBase
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used by source generator [LoggerMessaage]")]
    private readonly ILogger<LinkController> _logger;

    private readonly PeterPediaContext _dbContext;

    public LinkController(ILogger<LinkController> logger, PeterPediaContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var links = await _dbContext.Links.AsSplitQuery().ToListAsync().ConfigureAwait(false);

        var result = new List<Link>(links.Count);
        foreach (LinkEF link in links)
        {
            result.Add(ConvertToLink(link));
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Link link)
    {
        if (link is null)
        {
            return BadRequest();
        }

        LogPostLink(link);

        LinkEF? linkEF = await _dbContext.Links.FindAsync(link.Id);

        if (linkEF is null)
        {
            linkEF = new LinkEF();
        }

        linkEF.Title = link.Title;
        linkEF.Url = link.Url;
        
        if (linkEF.Id > 0)
        {
            LogLinkUpdate();
            _dbContext.Links.Update(linkEF);
        }
        else
        {
            LogLinkAdd();
            _dbContext.Links.Add(linkEF);
        }

        await _dbContext.SaveChangesAsync();
                
        LogLinkSaved(linkEF);

        return Ok(ConvertToLink(linkEF));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        LogDeleteLink(id);
        if (id <= 0)
        {
            return BadRequest();
        }

        var linkEF = await _dbContext.Links.Where(l => l.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

        if (linkEF is null)
        {
            LogNotFound(id);
            return NotFound();
        }

        _dbContext.Links.Remove(linkEF);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);

        LogLinkDeleted(linkEF);

        return Ok();
    }

    private static Link ConvertToLink(LinkEF linkEF)
    {
        if (linkEF is null)
        {
            throw new ArgumentNullException(nameof(linkEF));
        }

        var link = new Link()
        {
            Id = linkEF.Id,
            Title = linkEF.Title,
            Url = linkEF.Url,
        };
       
        return link;
    }

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter
    [LoggerMessage(0, LogLevel.Debug, "Delete link with id {id}.")]
    partial void LogDeleteLink(int id);

    [LoggerMessage(1, LogLevel.Debug, "Link with id {id} not found.")]
    partial void LogNotFound(int id);

    [LoggerMessage(2, LogLevel.Debug, "Deleted link {link}")]
    partial void LogLinkDeleted(LinkEF link);

    [LoggerMessage(3, LogLevel.Debug, "Post link {link}")]
    partial void LogPostLink(Link link);
    
    [LoggerMessage(4, LogLevel.Debug, "Update link.")]
    partial void LogLinkUpdate();

    [LoggerMessage(5, LogLevel.Debug, "Add new link")]
    partial void LogLinkAdd();

    [LoggerMessage(6, LogLevel.Debug, "Link {link} saved to database.")]
    partial void LogLinkSaved(LinkEF link);
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
}