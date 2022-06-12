namespace PeterPedia.Services;

public class EpisodeManager : IEpisodeManager
{
    private readonly ILogger<EpisodeManager> _logger;
    private readonly PeterPediaContext _dbContext;
    private readonly TheMovieDatabaseService _tmdbService;
    private readonly IFileService _fileService;
    private readonly IConfiguration _configuration;

    public EpisodeManager(
        ILogger<EpisodeManager> logger,
        PeterPediaContext dbContext,
        TheMovieDatabaseService tmdbService,
        IFileService fileService,
        IConfiguration configuration)
    {
        _logger = logger;
        _dbContext = dbContext;
        _tmdbService = tmdbService;
        _fileService = fileService;
        _configuration = configuration;
    }

    public async Task<Result<string>> AddAsync(int showId)
    {
        LogMessage.EpisodeAdd(_logger, showId);

        if (showId == 0)
        {
            LogMessage.EpisodeAddFailed(_logger, showId, "Id on show to add can't be 0.");
            return new ErrorResult<string>("Id on movie to add can't be 0.");
        }

        ShowEF? show = await _dbContext.Shows.FindAsync(showId).ConfigureAwait(false);

        if (show is not null)
        {
            LogMessage.EpisodeAddFailed(_logger, showId, "Show already exists");
            return new ErrorResult<string>("Show already exists");
        }

        TMDbShow? tmdbShow = await _tmdbService.GetTvShowAsync(showId, null).ConfigureAwait(false);

        if (tmdbShow is null)
        {
            LogMessage.TheMovieDbFailed(_logger);
            LogMessage.EpisodeAddFailed(_logger, showId, "Failed to fetch data from themoviedb.org");
            return new ErrorResult<string>("Failed to fetch data from themoviedb.org");
        }

        await DownloadCoverAsync(tmdbShow.Id, tmdbShow.PosterPath);

        show = new ShowEF()
        {
            Id = tmdbShow.Id,
            Title = tmdbShow.Title,
            ETag = tmdbShow.ETag,
            Status = tmdbShow.Status,
            LastUpdate = DateTime.UtcNow,
        };

        foreach (TMDbSeason tmdbSeason in tmdbShow.Seasons)
        {
            var season = new SeasonEF()
            {
                SeasonNumber = tmdbSeason.SeasonNumber,
            };

            foreach (TMDbEpisode tmdbEpisode in tmdbSeason.Episodes)
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

        return new SuccessResult<string>($"Added show {show.Title}.");
    }

    public async Task<Result> DeleteAsync(int id)
    {
        LogMessage.EpisodeDelete(_logger, id);

        ShowEF? show = await _dbContext.Shows.Where(s => s.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);
        if (show is null)
        {
            LogMessage.EpisodeDeleteFailed(_logger, id, "Show not found");
            return new NotFoundResult();
        }

        _dbContext.Shows.Remove(show);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);

        return new SuccessResult();
    }

    public async Task<Result<IList<Show>>> GetAllAsync()
    {
        List<ShowEF> shows = await _dbContext.Shows
            .Include(sh => sh.Seasons)
            .ThenInclude(se => se.Episodes)
            .AsSplitQuery()
            .ToListAsync();

        var result = new List<Show>(shows.Count);
        foreach (ShowEF? show in shows)
        {
            result.Add(ConvertToShow(show));
        }

        return new SuccessResult<IList<Show>>(result);
    }

    public async Task<Result<IList<Show>>> GetWatchlistAsync()
    {
        List<ShowEF> shows = await _dbContext.Shows
            .Include(sh => sh.Seasons)
            .ThenInclude(se => se.Episodes)
            .OrderBy(s => s.Title)
            .AsSplitQuery()
            .ToListAsync();

        var result = new List<Show>(shows.Count);
        foreach (ShowEF? showEF in shows)
        {
            Show show = ConvertToShow(showEF);

            show.Calculate();
            if (show.UnwatchedEpisodeCount > 0)
            {
                result.Add(show);
            }
        }

        return new SuccessResult<IList<Show>>(result);
    }

    public async Task<Result<Show>> GetAsync(int id)
    {
        ShowEF? show = await _dbContext.Shows
            .Include(sh => sh.Seasons)
            .ThenInclude(se => se.Episodes)
            .AsSplitQuery()
            .Where(s => s.Id == id)
            .SingleOrDefaultAsync();

        return show is null
            ? new NotFoundResult<Show>()
            : new SuccessResult<Show>(ConvertToShow(show));
    }

    public async Task<Result<IList<Episode>>> GetEpisodesAsync()
    {
        DateTime limit = DateTime.UtcNow.AddDays(-28);
        DateTime maxLimit = DateTime.UtcNow.AddDays(7);

        List<EpisodeEF> episodes = await _dbContext.Episodes
            .Include(e => e.Season)
            .ThenInclude(s => s.Show)
            .AsSplitQuery()
            .Where(e => e.AirDate > limit && e.AirDate < maxLimit)
            .OrderByDescending(e => e.AirDate).ToListAsync();

        var result = new List<Episode>(episodes.Count);
        foreach (EpisodeEF episodeEF in episodes)
        {
            result.Add(ConvertToLatestEpisode(episodeEF));
        }

        return new SuccessResult<IList<Episode>>(result);
    }

    public async Task<Result<Show>> UpdateAsync(Show show)
    {
        LogMessage.EpisodeUpdate(_logger, show);

        ShowEF? existingShow = await _dbContext.Shows
            .Where(s => s.Id == show.Id)
            .AsTracking()
            .SingleOrDefaultAsync();

        if (existingShow is null)
        {
            LogMessage.EpisodeUpdateFailed(_logger, show, "Show not found.");
            return new NotFoundResult<Show>();
        }

        existingShow.Title = show.Title;

        _dbContext.Shows.Update(existingShow);
        await _dbContext.SaveChangesAsync();

        if (show.ForceRefresh)
        {
            await RefreshShowAsync(show.Id, etag: null);
        }

        return new SuccessResult<Show>(ConvertToShow(existingShow));
    }

    public async Task<Result> WatchAsync(ShowWatchData watchData)
    {
        LogMessage.EpisodeWatch(_logger, watchData);

        SeasonEF? season = null;
        EpisodeEF? episode = null;
        if (watchData.SeasonId.HasValue)
        {
            season = await _dbContext.Seasons
                .Include(s => s.Episodes)
                .AsSplitQuery()
                .AsTracking()
                .Where(s => s.Id == watchData.SeasonId)
                .OrderBy(s => s.Id)
                .SingleOrDefaultAsync();

            if (season is null)
            {
                LogMessage.EpisodeWatchFailed(_logger, watchData, "Season not found.");

                return new NotFoundResult();
            }

            foreach (EpisodeEF seasonEpisode in season.Episodes)
            {
                seasonEpisode.Watched = watchData.Watched;
            }

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return new SuccessResult();
        }
        else
        {
            episode = await _dbContext.Episodes
                .Where(e => e.Id == watchData.EpisodeId)
                .AsTracking()
                .SingleOrDefaultAsync();

            if (episode is null)
            {
                LogMessage.EpisodeWatchFailed(_logger, watchData, "Episode not found.");

                return new NotFoundResult();
            }

            episode.Watched = watchData.Watched;
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return new SuccessResult();
        }
    }

    public async Task RefreshAsync()
    {
        List<ShowEF> shows = await _dbContext.Shows.Include(sh => sh.Seasons).ThenInclude(se => se.Episodes).AsSplitQuery().OrderBy(s => s.Title).ToListAsync().ConfigureAwait(false);

        var updateCount = 0;
        foreach (ShowEF show in shows)
        {
            if (updateCount >= 10)
            {
                return;
            }

            if (ShouldUpdateShow(show))
            {
                await RefreshShowAsync(show.Id, show.ETag);

                updateCount += 1;
            }
        }
    }

    private static bool ShouldUpdateShow(ShowEF show)
    {
        if (show is null)
        {
            return false;
        }

        TimeSpan timeSinceLastUpdate = DateTime.UtcNow - show.LastUpdate;

        return show.Status.ToUpperInvariant() switch
        {
            "RETURNING SERIES" => timeSinceLastUpdate.TotalDays > 1.0,
            "PLANNED" or "PILOT" or "IN PRODUCTION" => timeSinceLastUpdate.TotalDays > 7.0,
            "ENDED" or "CANCELED" => timeSinceLastUpdate.TotalDays > 14.0,
            _ => timeSinceLastUpdate.TotalDays > 14.0,
        };
    }

    private async Task RefreshShowAsync(int id, string? etag)
    {
        ShowEF? show = await _dbContext.Shows
            .Include(show => show.Seasons)
            .ThenInclude(season => season.Episodes)
            .AsSplitQuery()
            .Where(show => show.Id == id)
            .AsTracking()
            .SingleOrDefaultAsync();

        if (show is null)
        {
            return;
        }

        TMDbShow? tmdbShow = null;
        try
        {
            tmdbShow = await _tmdbService.GetTvShowAsync(id, etag);
        }
        catch (InvalidOperationException)
        {
            LogMessage.TheMovieDbFailed(_logger);                
            return;
        }

        if (tmdbShow is null)
        {
            show.LastUpdate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return;
        }

        await DownloadCoverAsync(tmdbShow.Id, tmdbShow.PosterPath);

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
        await _dbContext.SaveChangesAsync();
    }

    private async Task DownloadCoverAsync(int id, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var url = string.Empty;

        try
        {
            var filename = Path.Combine(_configuration["ImagePath"], "episodes", $"{id}.jpg");

            if (!_fileService.FileExists(filename))
            {
                url = await _tmdbService.GetImageUrlAsync(path);

                if (!string.IsNullOrWhiteSpace(url))
                {
                    await _fileService.DownloadImageAsync(url, filename);
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage.FailedDownload(_logger, url, ex);
        }
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
            LastUpdate = showEF.LastUpdate,
        };

        foreach (SeasonEF seasonEF in showEF.Seasons)
        {
            var season = new Season()
            {
                Id = seasonEF.Id,
                SeasonNumber = seasonEF.SeasonNumber,
            };

            foreach (EpisodeEF episodeEF in seasonEF.Episodes)
            {
                season.Episodes.Add(ConvertToEpisode(episodeEF));
            }

            show.Seasons.Add(season);
        }

        show.Calculate();
        return show;
    }

    private static Episode ConvertToEpisode(EpisodeEF episodeEF)
    {
        return new Episode()
        {
            Id = episodeEF.Id,
            EpisodeNumber = episodeEF.EpisodeNumber,
            Title = episodeEF.Title,
            AirDate = episodeEF.AirDate,
            Watched = episodeEF.Watched,
        };
    }

    private static Episode ConvertToLatestEpisode(EpisodeEF episodeEF)
    {
        Episode episode = ConvertToEpisode(episodeEF);

        episode.ShowTitle = episodeEF.Season.Show.Title;
        episode.SeasonNumber = episodeEF.Season.SeasonNumber;

        return episode;
    }
}
