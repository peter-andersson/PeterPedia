using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using PeterPedia.Server.Data;
using PeterPedia.Server.Services;
using PeterPedia.Server.Data.Models;
using PeterPedia.Server.Services.Models;
using PeterPedia.Shared;

namespace PeterPedia.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TVController : Controller
    {
        private readonly ILogger<TVController> _logger;
        private readonly PeterPediaContext _dbContext;
        private readonly TheMovieDatabaseService _tmdbService;

        public TVController(ILogger<TVController> logger, PeterPediaContext dbContext, TheMovieDatabaseService tmdbService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _tmdbService = tmdbService;
        }

        [HttpGet("{id:int?}")]
        public async Task<IActionResult> Get(int? id)
        {
            _logger.LogDebug($"Get show id: {id}");
            if (id.GetValueOrDefault(0) > 0)
            {
                var showEF = await _dbContext.Shows.Include(s => s.Seasons).ThenInclude(e => e.Episodes).AsSplitQuery().Where(s => s.Id == id).SingleOrDefaultAsync().ConfigureAwait(false);

                if (showEF is null)
                {
                    return NotFound();
                }

                return Ok(ConvertToShow(showEF));
            }
            else
            {
                var shows = await _dbContext.Shows.Include(sh => sh.Seasons).ThenInclude(se => se.Episodes).AsSplitQuery().AsNoTracking().ToListAsync().ConfigureAwait(false);

                var result = new List<Show>(shows.Count);
                foreach (var show in shows)
                {
                    result.Add(ConvertToShow(show));
                }

                return Ok(result);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddShow data)
        {
            _logger.LogDebug($"Add tv show with id {data?.Id}");

            if (data is null)
            {
                return BadRequest();
            }

            if (data.Id == 0)
            {
                return BadRequest();
            }

            var show = await _dbContext.Shows.FindAsync(data.Id).ConfigureAwait(false);

            if (show is not null)
            {
                _logger.LogDebug("Show already exists");

                return Conflict();
            }

            var tmdbShow = await _tmdbService.GetTvShowAsync(data.Id, null).ConfigureAwait(false);

            if (tmdbShow is null)
            {
                _logger.LogError("Failed to fetch data from themoviedb.org");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            show = new ShowEF()
            {
                Id = tmdbShow.Id,
                Title = tmdbShow.Title,
                ETag = tmdbShow.ETag,
                Status = tmdbShow.Status,
                LastUpdate = DateTime.UtcNow,
            };

            foreach (var tmdbSeason in tmdbShow.Seasons)
            {
                var season = new SeasonEF()
                {
                    SeasonNumber = tmdbSeason.SeasonNumber,
                };

                foreach (var tmdbEpisode in tmdbSeason.Episodes)
                {
                    var episode = new EpisodeEF()
                    {
                        Title = tmdbEpisode.Title,
                        EpisodeNumber = tmdbEpisode.EpisodeNumber,
                        AirDate = tmdbEpisode.AirDate,
                        Watched = false
                    };

                    season.Episodes.Add(episode);
                }

                show.Seasons.Add(season);
            }

            _dbContext.Shows.Add(show);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return Ok(ConvertToShow(show));
        }

        [HttpPost("watch")]
        public async Task<IActionResult> WatchEpisode([FromBody] ShowWatchData data)
        {
            if (data is null)
            {
                return BadRequest("Missing data in body");
            }

            if (!data.EpisodeId.HasValue && !data.SeasonId.HasValue)
            {
                return BadRequest("EpisodeId or SeasonId must be set.");
            }

            if (data.Watched)
            {
                if (data.SeasonId.HasValue)
                {
                    _logger.LogDebug($"Mark season as watched {data.SeasonId}");
                    var season = await _dbContext.Seasons.Include(s => s.Episodes).AsSplitQuery().Where(s => s.Id == data.SeasonId).OrderBy(s => s.Id).SingleOrDefaultAsync().ConfigureAwait(false);

                    if (season is null)
                    {
                        _logger.LogDebug("Season not found");
                        return NotFound();
                    }

                    foreach (var episode in season.Episodes)
                    {
                        episode.Watched = true;
                    }

                    await _dbContext.SaveChangesAsync().ConfigureAwait(false);

                    return Ok();
                }
                else
                {
                    _logger.LogDebug($"Mark episode as watched {data.EpisodeId}");
                    var episode = await _dbContext.Episodes.FindAsync(data.EpisodeId).ConfigureAwait(false);

                    if (episode is null)
                    {
                        _logger.LogDebug("Episode not found");
                        return NotFound();
                    }

                    episode.Watched = true;
                    await _dbContext.SaveChangesAsync().ConfigureAwait(false);

                    return Ok();
                }
            }
            else
            {
                if (data.SeasonId.HasValue)
                {
                    _logger.LogDebug($"Mark season as unwatched {data.SeasonId}");
                    var season = await _dbContext.Seasons.Include(s => s.Episodes).AsSplitQuery().Where(s => s.Id == data.SeasonId).OrderBy(s => s.Id).SingleOrDefaultAsync().ConfigureAwait(false);

                    if (season is null)
                    {
                        _logger.LogDebug("Season not found");
                        return NotFound();
                    }

                    foreach (var episode in season.Episodes)
                    {
                        episode.Watched = false;
                    }

                    await _dbContext.SaveChangesAsync().ConfigureAwait(false);
                    return Ok();
                }
                else
                {
                    _logger.LogDebug($"Mark episode as unwatched {data.EpisodeId}");
                    var episode = await _dbContext.Episodes.FindAsync(data.EpisodeId).ConfigureAwait(false);

                    if (episode is null)
                    {
                        _logger.LogDebug("Episode not found");
                        return NotFound();
                    }

                    episode.Watched = false;
                    await _dbContext.SaveChangesAsync().ConfigureAwait(false);

                    return Ok();
                }
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Show show)
        {
            if (show is null)
            {
                return BadRequest();
            }

            var existingShow = await _dbContext.Shows.FindAsync(show.Id).ConfigureAwait(false);

            if (existingShow is null)
            {
                return NotFound();
            }

            existingShow.Title = show.Title;

            _dbContext.Shows.Update(existingShow);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            if (show.ForceRefresh)
            {
                await UpdateShow(show.Id, true).ConfigureAwait(false);
            }

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogDebug($"Delete show with id {id}");
            var show = await _dbContext.Shows.FindAsync(id).ConfigureAwait(false);
            if (show is null)
            {
                return NotFound();
            }

            _dbContext.Shows.Remove(show);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            return Ok();
        }

        private async Task UpdateShow(int id, bool forceRefresh)
        {
            _logger.LogInformation($"TVController: Updating tv show with id {id} - Force refresh {forceRefresh}");

            var show = await _dbContext.Shows
                .Include(show => show.Seasons)
                .ThenInclude(season => season.Episodes)
                .AsSplitQuery()
                .Where(show => show.Id == id)
                .SingleOrDefaultAsync().ConfigureAwait(false);

            if (show is null)
            {
                _logger.LogError("TVController: Show not found");
                return;
            }

            string? etag = show.ETag;
            if (forceRefresh)
            {
                etag = null;
            }

            TMDbShow? tmdbShow = null;
            try
            {
                tmdbShow = await _tmdbService.GetTvShowAsync(id, etag).ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                _logger.LogError("TVController: Failed to fetch data from themoviedb.org");
                return;
            }

            if (tmdbShow is null)
            {
                _logger.LogInformation("TVController: Show not changed");

                show.LastUpdate = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);

                return;
            }

            show.ETag = tmdbShow.ETag;
            show.Status = tmdbShow.Status;

            foreach (var tmdbSeason in tmdbShow.Seasons)
            {
                var season = show.Seasons.Where(s => s.SeasonNumber == tmdbSeason.SeasonNumber).SingleOrDefault();
                if (season is null)
                {
                    season = new SeasonEF()
                    {
                        SeasonNumber = tmdbSeason.SeasonNumber,
                    };

                    show.Seasons.Add(season);
                }

                foreach (var tmdbEpisode in tmdbSeason.Episodes)
                {
                    var episode = season.Episodes.Where(e => e.EpisodeNumber == tmdbEpisode.EpisodeNumber).SingleOrDefault();
                    if (episode is null)
                    {
                        episode = new EpisodeEF()
                        {
                            EpisodeNumber = tmdbEpisode.EpisodeNumber,
                            Watched = false
                        };

                        season.Episodes.Add(episode);
                    }

                    episode.Title = tmdbEpisode.Title;
                    episode.AirDate = tmdbEpisode.AirDate;
                }
            }

            show.LastUpdate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private static Show ConvertToShow(ShowEF showEF)
        {
            if (showEF is null)
            {
                throw new ArgumentNullException(nameof(showEF));
            }

            var show = new Show()
            {
                Id = showEF.Id,
                Title = showEF.Title,
                Status = showEF.Status,
                TheMovieDbUrl = $"https://www.themoviedb.org/tv/{showEF.Id}",
            };

            foreach (var seasonEF in showEF.Seasons)
            {
                var season = new Season()
                {
                    Id = seasonEF.Id,
                    SeasonNumber = seasonEF.SeasonNumber,
                };

                foreach (var episodeEF in seasonEF.Episodes)
                {
                    var episode = new Episode()
                    {
                        Id = episodeEF.Id,
                        EpisodeNumber = episodeEF.EpisodeNumber,
                        Title = episodeEF.Title,
                        AirDate = episodeEF.AirDate,
                        Watched = episodeEF.Watched,
                    };

                    season.Episodes.Add(episode);
                }

                show.Seasons.Add(season);
            }

            show.Calculate();
            return show;
        }
    }
}
