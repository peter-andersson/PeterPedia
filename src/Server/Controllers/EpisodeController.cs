using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace PeterPedia.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class EpisodeController : Controller
{
    private readonly IEpisodeManager _episodeManager;
    public EpisodeController(IEpisodeManager episodeManager) => _episodeManager = episodeManager;    
    
    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] string lastupdate)
    {
        if (!DateTime.TryParseExact(lastupdate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastUpdated))
        {
            return BadRequest("Invalid date format");
        }

        Result<IList<Show>> result = await _episodeManager.GetAsync(lastUpdated);

        return result.Success ? Ok(result.Data) : StatusCode(500);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetShowAsync(int id)
    {
        Result<Show> result = await _episodeManager.GetAsync(id);

        return result.Success ? Ok(result.Data) : StatusCode(500);
    }

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeletedAsync([FromQuery] string deleted)
    {
        if (!DateTime.TryParseExact(deleted, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime since))
        {
            return BadRequest("Invalid date format");
        }

        Result<IList<DeleteLog>> result = await _episodeManager.GetDeletedAsync(since);

        return result.Success ? Ok(result.Data) : StatusCode(500);
    }

    [HttpGet("episodes")]
    public async Task<IActionResult> GetEpisodesAsync()
    {
        Result<IList<Episode>> result = await _episodeManager.GetEpisodesAsync();

        return result.Success ? Ok(result.Data) : StatusCode(500);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] AddShow data)
    {
        if (data is null)
        {
            return BadRequest();
        }

        Result<Show> result = await _episodeManager.AddAsync(data);

        return result switch
        {
            SuccessResult<Show> successResult => Ok(successResult.Data),
            ConflictResult<Show> => Conflict(),
            _ => StatusCode(500)
        };
    }

    [HttpPost("watch")]
    public async Task<IActionResult> WatchEpisodeAsync([FromBody] ShowWatchData data)
    {
        if (data is null)
        {
            return BadRequest("Missing data in body");
        }

        if (!data.EpisodeId.HasValue && !data.SeasonId.HasValue)
        {
            return BadRequest("EpisodeId or SeasonId must be set.");
        }

        Result result = await _episodeManager.Watch(data);
        return result switch
        {
            SuccessResult => NoContent(),
            Services.NotFoundResult => NotFound(),
            _ => StatusCode(500)
        };
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync([FromBody] Show show)
    {
        if (show is null)
        {
            return BadRequest();
        }

        Result<Show> result = await _episodeManager.UpdateAsync(show);
        return result switch
        {
            SuccessResult<Show> successResult => Ok(successResult.Data),
            NotFoundResult<Show> => NotFound(),
            _ => StatusCode(500)
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        Result result = await _episodeManager.DeleteAsync(id);
        return result switch
        {
            SuccessResult => NoContent(),
            Services.NotFoundResult => NotFound(),
            _ => StatusCode(500)
        };
    }
}
