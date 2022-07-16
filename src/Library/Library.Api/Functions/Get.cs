using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Library.Api.Functions;

public class Get
{
    private readonly IDataStorage<BookEntity> _dataStoreage;

    public Get(IDataStorage<BookEntity> dataStorage) => _dataStoreage = dataStorage;

    [FunctionName("Get")]    
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get/{id}")] HttpRequest req,
        string id,
        CancellationToken _)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return req.BadRequest("Missing query parameter id");
        }

        BookEntity? book = await _dataStoreage.GetAsync(id, id);

        return book is not null ? req.Ok(book.ConvertToBook()) : req.NotFound();
    }
}
