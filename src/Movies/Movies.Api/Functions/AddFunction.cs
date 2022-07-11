using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TheMovieDatabase;
using TheMovieDatabase.Models;
using Movies.Api.Models;
using Movies.Api.Data;

namespace Movies.Api.Functions;

public class AddFunction
{
    private readonly ITheMovieDatabaseService _service;
    private readonly CosmosContext _dbContext;

    public AddFunction(ITheMovieDatabaseService service, CosmosContext dbContext)
    {
        _service = service;
        _dbContext = dbContext;
    }

    [FunctionName("Add")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Azure function default name")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        string id = req.Query["id"];
        if (!int.TryParse(id, out var movieId))
        {
            return new BadRequestObjectResult("Missing query parameter id");
        }

        MovieEntity movie = await _dbContext.GetAsync(id);

        if (movie is not null)
        {
            return new ConflictResult();
        }

        TMDbMovie tmdbMovie = await _service.GetMovieAsync(movieId, string.Empty);

        if (tmdbMovie is null)
        {
            log.LogError("Failed to fetch data for movie with id {0} from themoviedb.org", id);
            return new StatusCodeResult(500);
        }

        // TODO: Ladda hem poster
        // await DownloadCoverAsync(tmdbMovie.Id, tmdbMovie.PosterPath);

        movie = new MovieEntity()
        {
            Id = id,
            ImdbId = tmdbMovie.ImdbId ?? string.Empty,
            OriginalLanguage = tmdbMovie.OriginalLanguage,
            OriginalTitle = tmdbMovie.OriginalTitle,
            ReleaseDate = tmdbMovie.ReleaseDate,
            RunTime = tmdbMovie.RunTime,
            Title = tmdbMovie.Title,
            WatchedDate = null,
            ETag = tmdbMovie.ETag
        };

        await _dbContext.AddAsync(movie);

        log.LogInformation($"Added movie with id {0} and title {1}", movie.Id, movie.Title);

        return new OkResult();
    }
}
