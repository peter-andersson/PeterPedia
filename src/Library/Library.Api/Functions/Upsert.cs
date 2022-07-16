using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace Library.Api.Functions;

public class Upsert
{
    private readonly ILogger<Upsert> _log;
    private readonly IDataStorage<BookEntity> _dataStorage;
    private readonly IFileStorage _fileStorage;
    private readonly IHttpClientFactory _clientFactory;

    public Upsert(ILogger<Upsert> log, IDataStorage<BookEntity> dbContext, IFileStorage fileStorage, IHttpClientFactory clientFactory)
    {
        _log = log;
        _dataStorage = dbContext;
        _fileStorage = fileStorage;
        _clientFactory = clientFactory;
    }

    [FunctionName("Upsert")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "upsert")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        Book book = JsonConvert.DeserializeObject<Book>(requestBody);

        if (book is null)
        {
            return req.BadRequest("Missing book object");
        }

        BookEntity? existing = await _dataStorage.GetAsync(book.Id);

        var update = true;

        if (existing is null)
        {
            update = false;
            existing = new BookEntity()
            {
                Id = Guid.NewGuid().ToString(),
            };          
        }

        existing.Read = book.Read;
        existing.WantToRead = book.WantToRead;
        existing.Reading = book.Reading;
        existing.Title = book.Title;
        existing.Authors = book.Authors;

        try
        {
            if (update)
            {

                await _dataStorage.UpdateAsync(existing);
                _log.LogInformation("Updated book with id {id}, title {title}", existing.Id, existing.Title);                
            }
            else
            {
                await _dataStorage.AddAsync(existing);
                _log.LogInformation("Added book with id {id}, title {title}", existing.Id, existing.Title);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }

        if (!string.IsNullOrWhiteSpace(book.CoverUrl))
        {
            try
            {
                HttpClient httpClient = _clientFactory.CreateClient();

                using HttpResponseMessage response = await httpClient.GetAsync(book.CoverUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                var stream = new MemoryStream();           
                await response.Content.CopyToAsync(stream, cancellationToken);
                stream.Seek(0, SeekOrigin.Begin);

                await _fileStorage.UploadAsync($"{existing.Id}.jpg", stream);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to download cover image from {url}.", book.CoverUrl);
            }
        }

        return req.Ok();
    }
}
