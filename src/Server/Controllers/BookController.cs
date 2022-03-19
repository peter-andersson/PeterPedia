using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace PeterPedia.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class BookController : Controller
{
    private readonly ILogger<BookController> _logger;
    private readonly IBookManager _bookManager;

    public BookController(ILogger<BookController> logger, IBookManager bookManager)
    {
        _logger = logger;
        _bookManager = bookManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery]string lastupdate)
    {
        if (!DateTime.TryParseExact(lastupdate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastUpdated))
        {
            return BadRequest("Invalid date format");
        }

        IList<Book> result = await _bookManager.GetAsync(lastUpdated);

        return Ok(result);        
    }

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeletedAsync([FromQuery] string deleted)
    {
        if (!DateTime.TryParseExact(deleted, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime since))
        {
            return BadRequest("Invalid date format");
        }

        IList<DeleteLog> result = await _bookManager.GetDeletedAsync(since);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Book book)
    {
        if (book is null)
        {
            return BadRequest();
        }

        Log.BookAdd(_logger, book);

        BookResult result = await _bookManager.AddAsync(book);
        if (result.Success)
        {
            return Ok(result.Book);
        }
        else
        {
            Log.BookAddFailed(_logger, book, result.ErrorMessage);
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync([FromBody] Book book)
    {
        if (book is null)
        {
            return BadRequest();
        }

        Log.BookUpdate(_logger, book);

        BookResult result = await _bookManager.UpdateAsync(book);
        if (result.Success)
        {
            return Ok(result.Book);
        }
        else
        {
            Log.BookUpdateFailed(_logger, book, result.ErrorMessage);
            return NotFound();
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        Log.BookDelete(_logger, id);

        BookResult result = await _bookManager.DeleteAsync(id);
        if (result.Success)
        {
            return NoContent();
        }
        else
        {
            Log.BookDeleteFailed(_logger, id, result.ErrorMessage);
            return NotFound();
        }
    }
}
