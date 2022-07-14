using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Library.Api.Functions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure function runtime.")]
public class Delete
{
    private readonly ILogger<Delete> _log;
    private readonly IDataStorage<BookEntity> _dataStorage;    

    public Delete(ILogger<Delete> log, IDataStorage<BookEntity> dataStorage)
    {
        _log = log;
        _dataStorage = dataStorage;
    }

    [FunctionName("Delete")]    
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "delete/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return new BadRequestObjectResult("Missing query parameter id");
        }

        BookEntity? book = await _dataStorage.GetAsync(id, id);

        if (book is null)
        {
            return new NotFoundResult();
        }

        try
        {
            await _dataStorage.DeleteAsync(book);

            _log.LogInformation("Deleted book with id {id} and title {title}.", book.Id, book.Title);

            return new OkResult();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return new StatusCodeResult(500);
        }        
    }
}
