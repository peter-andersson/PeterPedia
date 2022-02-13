using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeterPedia.Server.Data;
using PeterPedia.Shared;
using PeterPedia.Server.Data.Models;
using PeterPedia.Server.Services;

namespace PeterPedia.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class VideoController : Controller
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used by source generator [LoggerMessaage]")]
    private readonly ILogger<VideoController> _logger;

    private readonly PeterPediaContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IFileService _fileService;

    public VideoController(ILogger<VideoController> logger, PeterPediaContext dbContext, IConfiguration configuration, IFileService fileService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _configuration = configuration;
        _fileService = fileService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var items = await _dbContext.Videos.ToListAsync().ConfigureAwait(false);

        var result = new List<Video>(items.Count);
        foreach (var item in items)
        {
            result.Add(ConvertToItem(item));
        }

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        LogDeleteVideo(id);

        var item = await _dbContext.Videos.Where(v => v.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

        if (item is null)
        {
            return NotFound();
        }

        try
        {
            _fileService.Delete(item.AbsolutePath);

            _dbContext.Videos.Remove(item);

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (IOException ioe)
        {
            LogDeleteFailed(id, ioe);

            return StatusCode(500);
        }

        return Ok();
    }

    private Video ConvertToItem(VideoEF itemEF)
    {
        if (itemEF is null)
        {
            throw new ArgumentNullException(nameof(itemEF));
        }

        var video = new Video()
        {
            Id = itemEF.Id,
            Title = itemEF.Title,
            Duration = itemEF.Duration,
            Type = itemEF.Type,
        };

        var basePath = _configuration["VideoPath"] ?? "/video";
        var relativePath = Path.GetRelativePath(basePath, itemEF.AbsolutePath);

        video.Url = "/video/" + relativePath.Replace('\\', '/');

        return video;
    }

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter
    [LoggerMessage(0, LogLevel.Debug, "Delete video with id {id}")]
    partial void LogDeleteVideo(int id);

    [LoggerMessage(1, LogLevel.Error, "Failed to delete video {id}.")]
    partial void LogDeleteFailed(int id, Exception e);
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression

}
