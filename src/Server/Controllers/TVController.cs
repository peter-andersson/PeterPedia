using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.EntityFrameworkCore;
using PeterPedia.Server.Data;
using PeterPedia.Server.Services;
using PeterPedia.Server.Data.Models;
using PeterPedia.Server.Services.Models;
using PeterPedia.Shared;

namespace PeterPedia.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class TVController : Controller
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used by source generator [LoggerMessaage]")]
    private readonly ILogger<TVController> _logger;

    private readonly PeterPediaContext _dbContext;
    private readonly TheMovieDatabaseService _tmdbService;

    public TVController(ILogger<TVController> logger, PeterPediaContext dbContext, TheMovieDatabaseService tmdbService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _tmdbService = tmdbService;
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var shows = await _dbContext.Shows.Include(sh => sh.Seasons).ThenInclude(se => se.Episodes).AsSplitQuery().ToListAsync().ConfigureAwait(false);

        var result = new List<Show>(shows.Count);
        foreach (var show in shows)
        {
            result.Add(ConvertToShow(show));
        }

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetShow(int id)
    {
        var show = await _dbContext.Shows.Where(s => s.Id == id).Include(sh => sh.Seasons).ThenInclude(se => se.Episodes).AsSplitQuery().FirstOrDefaultAsync().ConfigureAwait(false);

        if (show is null)
        {
            return NotFound();
        }

        return Ok(ConvertToShow(show));
    }    

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AddShow data)
    {
        if (data is null)
        {
            return BadRequest();
        }

        LogAddShow(data.Id);

        if (data.Id == 0)
        {
            return BadRequest();
        }

        var show = await _dbContext.Shows.FindAsync(data.Id).ConfigureAwait(false);

        if (show is not null)
        {
            return Conflict();
        }

        var tmdbShow = await _tmdbService.GetTvShowAsync(data.Id, null).ConfigureAwait(false);

        if (tmdbShow is null)
        {
            LogTheMovieDbFailed();
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
        LogShowAdded(show);

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

        LogWatchEpisode(data);

        SeasonEF? season = null;
        EpisodeEF? episode = null;
        if (data.SeasonId.HasValue)
        {
            season = await _dbContext.Seasons.Include(s => s.Episodes).AsSplitQuery().AsTracking().Where(s => s.Id == data.SeasonId).OrderBy(s => s.Id).SingleOrDefaultAsync().ConfigureAwait(false);

            if (season is null)
            {
                LogSeasonNotFound(data);

                return NotFound();
            }

            foreach (var seasonEpisode in season.Episodes)
            {
                seasonEpisode.Watched = data.Watched;
            }

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return Ok();
        }
        else
        {
            episode = await _dbContext.Episodes.Where(e => e.Id == data.EpisodeId).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

            if (episode is null)
            {
                LogEpisodeNotFound(data);

                return NotFound();
            }

            episode.Watched = data.Watched;
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return Ok();
        }
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Show show)
    {
        if (show is null)
        {
            return BadRequest();
        }

        LogUpdateShow(show);

        var existingShow = await _dbContext.Shows.Where(s => s.Id == show.Id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

        if (existingShow is null)
        {
            return NotFound();
        }

        existingShow.Title = show.Title;

        _dbContext.Shows.Update(existingShow);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        LogShowUpdated(existingShow);

        if (show.ForceRefresh)
        {
            await RefreshShow(show.Id).ConfigureAwait(false);
        }

        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        LogDeleteShow(id);
        var show = await _dbContext.Shows.Where(s => s.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);
        if (show is null)
        {
            LogNotFound(id);
            return NotFound();
        }

        _dbContext.Shows.Remove(show);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        LogShowDeleted(show);
        return Ok();
    }

    private async Task RefreshShow(int id)
    {
        LogRefreshShow(id);

        var show = await _dbContext.Shows
            .Include(show => show.Seasons)
            .ThenInclude(season => season.Episodes)
            .AsSplitQuery()
            .Where(show => show.Id == id)
            .AsTracking()
            .SingleOrDefaultAsync().ConfigureAwait(false);

        if (show is null)
        {
            return;
        }

        string? etag = null;

        TMDbShow? tmdbShow = null;
        try
        {
            tmdbShow = await _tmdbService.GetTvShowAsync(id, etag).ConfigureAwait(false);
        }
        catch (InvalidOperationException e)
        {
            LogRefreshFailed(id, e);
            return;
        }

        if (tmdbShow is null)
        {
            show.LastUpdate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return;
        }

        show.ETag = tmdbShow.ETag;
        show.Status = tmdbShow.Status;

        // Remove season no longer valid
        var i = 0;
        while (i < show.Seasons.Count)
        {
            SeasonEF season = show.Seasons[i];

            TMDbSeason? tmdbSeason = tmdbShow.Seasons.Where(s => s.SeasonNumber == season.SeasonNumber).SingleOrDefault();
            if (tmdbSeason is null)
            {
                show.Seasons.Remove(season);
                continue;
            }

            i += 1;
        }        

        foreach (TMDbSeason tmdbSeason in tmdbShow.Seasons)
        {
            SeasonEF? season = show.Seasons.Where(s => s.SeasonNumber == tmdbSeason.SeasonNumber).SingleOrDefault();
            if (season is null)
            {
                season = new SeasonEF()
                {
                    SeasonNumber = tmdbSeason.SeasonNumber,
                };

                show.Seasons.Add(season);
            }

            i = 0;
            while (i < season.Episodes.Count)
            {
                EpisodeEF episode = season.Episodes[i];

                TMDbEpisode? tmdbEpisode = tmdbSeason.Episodes.Where(e => e.EpisodeNumber == episode.EpisodeNumber).SingleOrDefault();
                if (tmdbEpisode is null)
                {
                    season.Episodes.Remove(episode);
                    continue;
                }

                i += 1;
            }

            foreach (TMDbEpisode? tmdbEpisode in tmdbSeason.Episodes)
            {
                EpisodeEF? episode = season.Episodes.Where(e => e.EpisodeNumber == tmdbEpisode.EpisodeNumber).SingleOrDefault();
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
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter
    [LoggerMessage(0, LogLevel.Debug, "Adding new show id {id}")]
    partial void LogAddShow(int id);

    [LoggerMessage(1, LogLevel.Error, "Failed to fetch data from themoviedb.org")]
    partial void LogTheMovieDbFailed();

    [LoggerMessage(2, LogLevel.Debug, "Show {show} added.")]
    partial void LogShowAdded(ShowEF show);

    [LoggerMessage(3, LogLevel.Debug, "Update show {show}")]
    partial void LogUpdateShow(Show show);

    [LoggerMessage(4, LogLevel.Debug, "Show {show} updated.")]
    partial void LogShowUpdated(ShowEF show);

    [LoggerMessage(5, LogLevel.Debug, "Book with id {id} not found.")]
    partial void LogNotFound(int id);

    [LoggerMessage(6, LogLevel.Debug, "Delete show with id {id}.")]
    partial void LogDeleteShow(int id);

    [LoggerMessage(7, LogLevel.Debug, "Delete show {show}.")]
    partial void LogShowDeleted(ShowEF show);

    [LoggerMessage(8, LogLevel.Debug, "TVController: Updating tv show with id {id}.")]
    partial void LogRefreshShow(int id);

    [LoggerMessage(9, LogLevel.Error, "TVController: Failed to fetch data from themoviedb.org for show {id}")]
    partial void LogRefreshFailed(int id, Exception e);

    [LoggerMessage(10, LogLevel.Debug, "Watch episode {data}")]
    partial void LogWatchEpisode(ShowWatchData data);

    [LoggerMessage(11, LogLevel.Debug, "Season not found {data}")]
    partial void LogSeasonNotFound(ShowWatchData data);

    [LoggerMessage(12, LogLevel.Debug, "Episode not found {data}")]
    partial void LogEpisodeNotFound(ShowWatchData data);
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression
}
