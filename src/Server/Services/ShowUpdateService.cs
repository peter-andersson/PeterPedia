using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PeterPedia.Server.Data;
using PeterPedia.Server.Data.Models;
using PeterPedia.Server.Services.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PeterPedia.Server.Services
{
    internal class ShowUpdateService : IShowUpdateService
    {
        private readonly ILogger<ShowUpdateService> _logger;
        private readonly PeterPediaContext _dbContext;
        private readonly TheMovieDatabaseService _tmdbService;

        public ShowUpdateService(ILogger<ShowUpdateService> logger, PeterPediaContext dbContext, TheMovieDatabaseService tmdbService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _tmdbService = tmdbService;
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Execute();

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task Execute()
        {
            _logger.LogInformation("ShowUpdateService - Execute");

            var shows = await _dbContext.Shows.Include(sh => sh.Seasons).ThenInclude(se => se.Episodes).AsSplitQuery().AsNoTracking().OrderBy(s => s.Title).ToListAsync().ConfigureAwait(false);

            int updateCount = 0;
            foreach (var show in shows)
            {
                if (updateCount >= 10)
                {
                    return;
                }

                if (UpdateShow(show))
                {
                    _logger.LogInformation($"Update tv show {show.Title}");
                    await UpdateShow(show.Id, false).ConfigureAwait(false);

                    updateCount += 1;
                }
            }
        }

        private bool UpdateShow(ShowEF show)
        {
            if (show is null)
            {
                return false;
            }

            var timeSinceLastUpdate = DateTime.UtcNow - show.LastUpdate;

            _logger.LogDebug($"Show {show.Title}, Status {show.Status}, LastUpdate {show.LastUpdate}, TimeSinceLast {timeSinceLastUpdate.TotalDays}");
            return (show.Status.ToUpperInvariant()) switch
            {
                "RETURNING SERIES" => timeSinceLastUpdate.TotalDays > 1.0,
                "PLANNED" or "PILOT" or "IN PRODUCTION" => timeSinceLastUpdate.TotalDays > 7.0,
                "ENDED" or "CANCELED" => timeSinceLastUpdate.TotalDays > 14.0,
                _ => timeSinceLastUpdate.TotalDays > 14.0,
            };
        }

        private async Task UpdateShow(int id, bool forceRefresh)
        {
            _logger.LogInformation($"Updating tv show with id {id} - Force refresh {forceRefresh}");

            var show = await _dbContext.Shows
                .Include(show => show.Seasons)
                .ThenInclude(season => season.Episodes)
                .AsSplitQuery()
                .Where(show => show.Id == id)
                .SingleOrDefaultAsync().ConfigureAwait(false);

            if (show is null)
            {
                _logger.LogError("Show not found");
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
                _logger.LogError("Failed to fetch data from themoviedb.org");
                return;
            }

            if (tmdbShow is null)
            {
                _logger.LogInformation("Show not changed");

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
    }
}