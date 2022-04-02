using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace PeterPedia.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class AuthorController : Controller
{    
    private readonly IAuthorManager _authorManager;

    public AuthorController(IAuthorManager authorManager) => _authorManager = authorManager;

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery]string lastupdate)
    {
        if (!DateTime.TryParseExact(lastupdate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastUpdated))
        {
            return BadRequest("Invalid date format");
        }

        Result<IList<Author>> result = await _authorManager.GetAsync(lastUpdated);

        return result.Success ? Ok(result.Data) : StatusCode(500);
    }

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeletedAsync([FromQuery] string deleted)
    {
        if (!DateTime.TryParseExact(deleted, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime since))
        {
            return BadRequest("Invalid date format");
        }

        Result<IList<DeleteLog>> result = await _authorManager.GetDeletedAsync(since);

        return result.Success ? Ok(result.Data) : StatusCode(500);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Author author)
    {
        if (author is null)
        {
            return BadRequest();
        }

        Result<Author> result = await _authorManager.AddAsync(author);

        return result switch
        {
            SuccessResult<Author> successResult => Ok(successResult.Data),
            ConflictResult<Author> => Conflict(),
            _ => StatusCode(500)
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        Result<Author> result = await _authorManager.DeleteAsync(id);

        return result switch
        {
            SuccessResult<Author> => NoContent(),
            NotFoundResult<Author> => NotFound(),
            _ => StatusCode(500)
        };
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync([FromBody] Author author)
    {
        if (author is null)
        {
            return BadRequest();
        }

        Result<Author> result = await _authorManager.UpdateAsync(author);
        return result switch
        {
            SuccessResult<Author> successResult => Ok(successResult.Data),
            ConflictResult<Author> => Conflict(),
            _ => StatusCode(500)
        };
    }    
}
