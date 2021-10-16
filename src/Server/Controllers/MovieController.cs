using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using PeterPedia.Server.Data;
using PeterPedia.Server.Services;
using PeterPedia.Server.Data.Models;
using PeterPedia.Shared;

namespace PeterPedia.Server.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class MovieController : Controller
    {
        private readonly ILogger<MovieController> _logger;
        private readonly PeterPediaContext _dbContext;
        private readonly TheMovieDatabaseService _tmdbService;

        public MovieController(ILogger<MovieController> logger, PeterPediaContext dbContext, TheMovieDatabaseService tmdbService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _tmdbService = tmdbService;
        }

        [HttpGet("{id:int?}")]
        public async Task<IActionResult> Get(int? id)
        {
            IQueryable<MovieEF> moviesIQ = from m in _dbContext.Movies
                                           select m;

            if (id.GetValueOrDefault(0) > 0)
            {
                var movie = await _dbContext.Movies.AsNoTracking().Where(m => m.Id == id).SingleOrDefaultAsync();
                if (movie is null)
                {
                    return NotFound();
                }

                return Ok(ConvertToMovie(movie));
            }
            else
            {
                var movies = await _dbContext.Movies.AsNoTracking().ToListAsync().ConfigureAwait(false);

                var result = new List<Movie>(movies.Count);
                foreach (var movie in movies)
                {
                    result.Add(ConvertToMovie(movie));
                }

                return Ok(result);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PostData data)
        {
            _logger.LogDebug($"Add movie with id {data?.Id}");

            if (data is null)
            {
                return BadRequest();
            }

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
                _logger.LogError("Failed to fetch data from themoviedb.org");
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

            return Ok(ConvertToMovie(movie));
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Movie movie)
        {
            if (movie is null)
            {
                return BadRequest();
            }

            var existingMovie = await _dbContext.Movies.FindAsync(movie.Id).ConfigureAwait(false);

            if (existingMovie is null)
            {
                return NotFound();
            }

            existingMovie.WatchedDate = movie.WatchedDate;
            existingMovie.Title = movie.Title;

            _dbContext.Movies.Update(existingMovie);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogDebug($"Delete movie with id {id}");
            var movie = await _dbContext.Movies.FindAsync(id).ConfigureAwait(false);
            if (movie is null)
            {
                return NotFound();
            }

            _dbContext.Movies.Remove(movie);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

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
    }
}
