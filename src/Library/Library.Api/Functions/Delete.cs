using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Library.Api.Functions;

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
        CancellationToken _)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return req.BadRequest("Missing query parameter id");
        }

        BookEntity? book = await _dataStorage.GetAsync(id, id);

        if (book is null)
        {
            return req.NotFound();
        }

        try
        {
            await _dataStorage.DeleteAsync(book);

            _log.LogInformation("Deleted book with id {id} and title {title}.", book.Id, book.Title);

            return req.Ok();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }        
    }
}
