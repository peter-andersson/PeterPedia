using PeterPedia.Server.Data;
using PeterPedia.Server.Data.Models;
using PeterPedia.Server.Services.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;
using PeterPedia.Server.Services;

namespace PeterPedia.Server.Jobs;

[DisallowConcurrentExecution]
public partial class ShowUpdateJob : IJob
{
#pragma warning disable IDE0052 // Remove unread private members
    private readonly ILogger<ShowUpdateJob> _logger;
#pragma warning restore IDE0052 // Remove unread private members
    private readonly PeterPediaContext _dbContext;
    private readonly TheMovieDatabaseService _tmdbService;

    public ShowUpdateJob(ILogger<ShowUpdateJob> logger, PeterPediaContext dbContext, TheMovieDatabaseService tmdbService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _tmdbService = tmdbService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        LogExecute();

        var shows = await _dbContext.Shows.Include(sh => sh.Seasons).ThenInclude(se => se.Episodes).AsSplitQuery().OrderBy(s => s.Title).ToListAsync().ConfigureAwait(false);

        int updateCount = 0;
        foreach (var show in shows)
        {
            if (updateCount >= 10)
            {
                return;
            }

            if (UpdateShow(show))
            {
                LogUpdateShow(show.Title, show.Id);
                await UpdateShow(show.Id).ConfigureAwait(false);

                updateCount += 1;
            }
        }
    }

    private static bool UpdateShow(ShowEF show)
    {
        if (show is null)
        {
            return false;
        }

        var timeSinceLastUpdate = DateTime.UtcNow - show.LastUpdate;

        return (show.Status.ToUpperInvariant()) switch
        {
            "RETURNING SERIES" => timeSinceLastUpdate.TotalDays > 1.0,
            "PLANNED" or "PILOT" or "IN PRODUCTION" => timeSinceLastUpdate.TotalDays > 7.0,
            "ENDED" or "CANCELED" => timeSinceLastUpdate.TotalDays > 14.0,
            _ => timeSinceLastUpdate.TotalDays > 14.0,
        };
    }

    private async Task UpdateShow(int id)
    {
        var show = await _dbContext.Shows
            .Include(show => show.Seasons)
            .ThenInclude(season => season.Episodes)
            .AsSplitQuery()
            .Where(show => show.Id == id)
            .AsTracking()
            .SingleOrDefaultAsync().ConfigureAwait(false);

        if (show is null)
        {
            LogShowNotFound(id);
            return;
        }

        string? etag = show.ETag;

        TMDbShow? tmdbShow = null;
        try
        {
            tmdbShow = await _tmdbService.GetTvShowAsync(id, etag).ConfigureAwait(false);
        }
        catch (InvalidOperationException e)
        {
            LogFailedToFetchFromTheMovieDb(id, e);
            return;
        }

        if (tmdbShow is null)
        {
            LogShowNotChanged(id);

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

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter
    [LoggerMessage(0, LogLevel.Debug, "ShowUpdateService - Execute")]
    partial void LogExecute();

    [LoggerMessage(1, LogLevel.Information, "Update tv show {title} [{id}]")]
    partial void LogUpdateShow(string title, int id);

    [LoggerMessage(2, LogLevel.Information, "Show not found [{id}]")]
    partial void LogShowNotFound(int id);

    [LoggerMessage(3, LogLevel.Error, "Failed to fetch data from themoviedb.org for show {id}")]
    partial void LogFailedToFetchFromTheMovieDb(int id, Exception e);

    [LoggerMessage(4, LogLevel.Debug, "Show [{id}] not changed.")]
    partial void LogShowNotChanged(int id);

#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression
}