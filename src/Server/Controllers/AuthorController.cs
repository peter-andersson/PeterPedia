using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace PeterPedia.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class AuthorController : Controller
{    
    private readonly ILogger<AuthorController> _logger;

    private readonly IAuthorManager _authorManager;

    public AuthorController(ILogger<AuthorController> logger, IAuthorManager authorManager)
    {
        _logger = logger;
        _authorManager = authorManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery]string lastupdate)
    {
        if (!DateTime.TryParseExact(lastupdate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastUpdated))
        {
            return BadRequest("Invalid date format");
        }        

        IList<Author> result = await _authorManager.GetAsync(lastUpdated);

        return Ok(result);
    }

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeletedAsync([FromQuery] string deleted)
    {
        if (!DateTime.TryParseExact(deleted, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime since))
        {
            return BadRequest("Invalid date format");
        }

        IList<DeleteLog> result = await _authorManager.GetDeletedAsync(since);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Author author)
    {
        if (author is null)
        {
            return BadRequest();
        }

        Log.AuthorAdd(_logger, author);

        AuthorResult result = await _authorManager.AddAsync(author);
        if (result.Success)
        {
            return Ok(result.Author);
        }
        else
        {
            Log.AuthorAddFailed(_logger, author, result.ErrorMessage);
            return Conflict();
        }        
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        Log.AuthorDelete(_logger, id);

        AuthorResult result = await _authorManager.DeleteAsync(id);
        if (result.Success)
        {
            return NoContent();
        }
        else
        {
            Log.AuthorDeleteFailed(_logger, id, result.ErrorMessage);
            return NotFound();
        }
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync([FromBody] Author author)
    {
        if (author is null)
        {
            return BadRequest();
        }

        Log.AuthorUpdate(_logger, author);

        AuthorResult result = await _authorManager.UpdateAsync(author);
        if (result.Success)
        {
            return Ok(result.Author);
        }
        else
        {
            Log.AuthorUpdateFailed(_logger, author, result.ErrorMessage);
            return NotFound();
        }
    }    
}
