using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace PeterPedia.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class MovieController : Controller
{
    private readonly IMovieManager _movieManager;

    public MovieController(IMovieManager movieManager) => _movieManager = movieManager;    

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] string lastupdate)
    {
        if (!DateTime.TryParseExact(lastupdate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastUpdated))
        {
            return BadRequest("Invalid date format");
        }

        Result<IList<Movie>> result = await _movieManager.GetAsync(lastUpdated);       

        return result.Success ? Ok(result) : StatusCode(500);
    }

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeletedAsync([FromQuery] string deleted)
    {
        if (!DateTime.TryParseExact(deleted, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime since))
        {
            return BadRequest("Invalid date format");
        }

        Result<IList<DeleteLog>> result = await _movieManager.GetDeletedAsync(since);

        return result.Success ? Ok(result) : StatusCode(500);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] AddMovie data)
    {
        if (data is null)
        {
            return BadRequest();
        }

        Result<Movie> result = await _movieManager.AddAsync(data);

        return result switch
        {
            SuccessResult<Movie> successResult => Ok(successResult.Data),
            ConflictResult<Movie> => Conflict(),
            _ => StatusCode(500)
        }
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync([FromBody] Movie movie)
    {
        if (movie is null)
        {
            return BadRequest();
        }

        Result<Movie> result = await _movieManager.UpdateAsync(movie);
        return result switch
        {
            SuccessResult<Movie> successResult => Ok(successResult.Data),
            NotFoundResult<Movie> => NotFound(),
            _ => StatusCode(500)
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        Result<Movie> result = await _movieManager.DeleteAsync(id);
        return result switch
        {
            SuccessResult<Movie> => NoContent(),
            NotFoundResult<Movie> => NotFound(),
            _ => StatusCode(500)
        };
    }    
}
