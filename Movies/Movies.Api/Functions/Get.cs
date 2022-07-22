using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Movies.Api.Functions;

public class Get
{
    private readonly IRepository _repository;

    public Get(IRepository repository) => _repository = repository;

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

        MovieEntity? movie = await _repository.GetAsync<MovieEntity>(id);

        return movie is not null ? req.Ok(movie.ConvertToMovie()) : req.NotFound();
    }
}
