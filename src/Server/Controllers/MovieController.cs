using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace PeterPedia.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class MovieController : Controller
{
    private readonly ILogger<MovieController> _logger;
    private readonly IMovieManager _movieManager;

    public MovieController(
        ILogger<MovieController> logger,
        IMovieManager movieManager)
    {
        _logger = logger;
        _movieManager = movieManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] string lastupdate)
    {
        if (!DateTime.TryParseExact(lastupdate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastUpdated))
        {
            return BadRequest("Invalid date format");
        }

        IList<Movie> result = await _movieManager.GetAsync(lastUpdated);       

        return Ok(result);
    }

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeletedAsync([FromQuery] string deleted)
    {
        if (!DateTime.TryParseExact(deleted, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime since))
        {
            return BadRequest("Invalid date format");
        }

        IList<DeleteLog> result = await _movieManager.GetDeletedAsync(since);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] AddMovie data)
    {
        if (data is null)
        {
            return BadRequest();
        }

        LogMessage.AddMovie(_logger, data.Id);

        if (data.Id == 0)
        {
            return BadRequest();
        }

        MovieResult result = await _movieManager.AddAsync(data);
        if (result.Success)
        {
            return Ok(result.Movie);
        }
        else
        {
            LogMessage.MovieAddFailed(_logger, data, result.ErrorMessage);
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync([FromBody] Movie movie)
    {
        if (movie is null)
        {
            return BadRequest();
        }

        LogMessage.MovieUpdate(_logger, movie);

        MovieResult result = await _movieManager.UpdateAsync(movie);
        if (result.Success)
        {
            return Ok(result.Movie);
        }
        else
        {
            LogMessage.MovieUpdateFailed(_logger, movie, result.ErrorMessage);
            return NotFound();
        }        
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        LogMessage.MovieDelete(_logger, id);

        MovieResult result = await _movieManager.DeleteAsync(id);
        if (result.Success)
        {
            return NoContent();
        }
        else
        {
            LogMessage.MovieDeleteFailed(_logger, id, result.ErrorMessage);
            return NotFound();
        }        
    }    
}
