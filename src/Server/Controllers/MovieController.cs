using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.EntityFrameworkCore;
using PeterPedia.Server.Data;
using PeterPedia.Server.Services;
using PeterPedia.Server.Data.Models;
using PeterPedia.Shared;

namespace PeterPedia.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
public partial class MovieController : Controller
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used by source generator [LoggerMessaage]")]
    private readonly ILogger<MovieController> _logger;

    private readonly PeterPediaContext _dbContext;
    private readonly TheMovieDatabaseService _tmdbService;

    public MovieController(ILogger<MovieController> logger, PeterPediaContext dbContext, TheMovieDatabaseService tmdbService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _tmdbService = tmdbService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var movies = await _dbContext.Movies.ToListAsync().ConfigureAwait(false);

        var result = new List<Movie>(movies.Count);
        foreach (var movie in movies)
        {
            result.Add(ConvertToMovie(movie));
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AddMovie data)
    {
        if (data is null)
        {
            return BadRequest();
        }

        LogAddMovie(data.Id);

        if (data.Id == 0)
        {
            return BadRequest();
        }

        var movie = await _dbContext.Movies.FindAsync(data.Id).ConfigureAwait(false);

        if (movie is not null)
        {
            return Conflict();
        }

        var tmdbMovie = await _tmdbService.GetMovieAsync(data.Id).ConfigureAwait(false);

        if (tmdbMovie is null)
        {
            LogTheMovieDbFailed();
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        movie = new MovieEF()
        {
            Id = tmdbMovie.Id,
            ImdbId = tmdbMovie.ImdbId,
            OriginalLanguage = tmdbMovie.OriginalLanguage,
            OriginalTitle = tmdbMovie.OriginalTitle,
            ReleaseDate = tmdbMovie.ReleaseDate,
            RunTime = tmdbMovie.RunTime,
            Title = tmdbMovie.Title,
            WatchedDate = null,
        };

        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        LogMovieAdded(movie);

        return Ok(ConvertToMovie(movie));
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Movie movie)
    {
        if (movie is null)
        {
            return BadRequest();
        }

        LogUpdateMovie(movie);

        var existingMovie = await _dbContext.Movies.Where(m => m.Id == movie.Id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

        if (existingMovie is null)
        {
            LogNotFound(movie.Id);
            return NotFound();
        }

        existingMovie.WatchedDate = movie.WatchedDate;
        existingMovie.Title = movie.Title;

        _dbContext.Movies.Update(existingMovie);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        LogMovieUpdated(existingMovie);

        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        LogDeleteMovie(id);
        var movie = await _dbContext.Movies.Where(m => m.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);
        if (movie is null)
        {
            LogNotFound(id);
            return NotFound();
        }

        _dbContext.Movies.Remove(movie);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        LogMovieDeleted(movie);

        return Ok();
    }

    private static Movie ConvertToMovie(MovieEF movieEF)
    {
        if (movieEF is null)
        {
            throw new ArgumentNullException(nameof(movieEF));
        }

        var movie = new Movie()
        {
            Id = movieEF.Id,
            OriginalLanguage = movieEF.OriginalLanguage,
            OriginalTitle = movieEF.OriginalTitle,
            Title = movieEF.Title,
            RunTime = movieEF.RunTime,
            ReleaseDate = movieEF.ReleaseDate,
            WatchedDate = movieEF.WatchedDate,
            ImdbUrl = $"https://www.imdb.com/title/{movieEF.ImdbId}",
            TheMovieDbUrl = $"https://www.themoviedb.org/movie/{movieEF.Id}",
        };

        return movie;

    }

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter
    [LoggerMessage(0, LogLevel.Debug, "Adding new movie {id}")]
    partial void LogAddMovie(int id);

    [LoggerMessage(1, LogLevel.Error, "Failed to fetch data from themoviedb.org")]
    partial void LogTheMovieDbFailed();

    [LoggerMessage(2, LogLevel.Debug, "Movie {movie} added.")]
    partial void LogMovieAdded(MovieEF movie);

    [LoggerMessage(3, LogLevel.Debug, "Update movie {movie}")]
    partial void LogUpdateMovie(Movie movie);

    [LoggerMessage(4, LogLevel.Debug, "Movie {movie} updated.")]
    partial void LogMovieUpdated(MovieEF movie);

    [LoggerMessage(5, LogLevel.Debug, "Movie with id {id} not found.")]
    partial void LogNotFound(int id);

    [LoggerMessage(6, LogLevel.Debug, "Delete movie with id {id}.")]
    partial void LogDeleteMovie(int id);

    [LoggerMessage(7, LogLevel.Debug, "Delete movie {movie}.")]
    partial void LogMovieDeleted(MovieEF movie);
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression
}
