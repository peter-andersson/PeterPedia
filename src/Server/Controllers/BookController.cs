using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace PeterPedia.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class BookController : Controller
{
    private readonly IBookManager _bookManager;

    public BookController(IBookManager bookManager) => _bookManager = bookManager;

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery]string lastupdate)
    {
        if (!DateTime.TryParseExact(lastupdate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastUpdated))
        {
            return BadRequest("Invalid date format");
        }

        Result<IList<Book>> result = await _bookManager.GetAsync(lastUpdated);

        return result switch
        {
            SuccessResult<IList<Book>> successResult => Ok(successResult.Data),
            _ => StatusCode(500)
        };
    }

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeletedAsync([FromQuery] string deleted)
    {
        if (!DateTime.TryParseExact(deleted, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime since))
        {
            return BadRequest("Invalid date format");
        }

        Result<IList<DeleteLog>> result = await _bookManager.GetDeletedAsync(since);

        return result switch
        {
            SuccessResult<IList<DeleteLog>> successResult => Ok(successResult.Data),
            _ => StatusCode(500)
        };
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Book book)
    {
        if (book is null)
        {
            return BadRequest();
        }
        
        Result<Book> result = await _bookManager.AddAsync(book);

        return result.Success ? Ok(result.Data) : StatusCode(500);
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync([FromBody] Book book)
    {
        if (book is null)
        {
            return BadRequest();
        }

        Result<Book> result = await _bookManager.UpdateAsync(book);

        return result switch
        {
            SuccessResult<Book> successResult => Ok(successResult.Data),
            NotFoundResult<Book> => NotFound(),
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
        
        Result<Book> result = await _bookManager.DeleteAsync(id);

        return result switch
        {
            SuccessResult<Book> => NoContent(),
            NotFoundResult<Book> => NotFound(),
            _ => StatusCode(500)
        };        
    }
}
