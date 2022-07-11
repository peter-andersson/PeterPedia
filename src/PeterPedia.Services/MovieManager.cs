using Microsoft.EntityFrameworkCore;
using TheMovieDatabase;
using TheMovieDatabase.Models;

namespace PeterPedia.Services;

public class MovieManager : IMovieManager
{
    private readonly ILogger<MovieManager> _logger;
    private readonly PeterPediaContext _dbContext;
    private readonly IFileService _fileService;
    private readonly IConfiguration _configuration;
    private readonly TheMovieDatabaseService _tmdbService;

    public MovieManager(
        ILogger<MovieManager> logger,
        PeterPediaContext dbContext,
        IFileService fileService,
        IConfiguration configuration,
        TheMovieDatabaseService tmdbService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _fileService = fileService;
        _configuration = configuration;
        _tmdbService = tmdbService;
    }

    public async Task<Result<string>> AddAsync(int id)
    {
        LogMessage.MovieAdd(_logger, id);

        if (id == 0)
        {
            LogMessage.MovieAddFailed(_logger, id, "Id on movie to add can't be 0.");

            return new ErrorResult<string>("Id on movie to add can't be 0.");
        }

        MovieEF? movie = await _dbContext.Movies.FindAsync(id);

        if (movie is not null)
        {
            LogMessage.MovieAddFailed(_logger, id, "Movie already exists");
            return new ErrorResult<string>("Movie already exists");
        }

        TMDbMovie? tmdbMovie = await _tmdbService.GetMovieAsync(id.ToString(), string.Empty);

        if (tmdbMovie is null)
        {
            LogMessage.TheMovieDbFailed(_logger);
            LogMessage.MovieAddFailed(_logger, id, "Failed to fetch data from themoviedb.org");
            return new ErrorResult<string>("Failed to fetch data from themoviedb.org");
        }

        await DownloadCoverAsync(tmdbMovie.Id, tmdbMovie.PosterPath);

        movie = new MovieEF()
        {
            Id = tmdbMovie.Id,
            ImdbId = tmdbMovie.ImdbId ?? string.Empty,
            OriginalLanguage = tmdbMovie.OriginalLanguage,
            OriginalTitle = tmdbMovie.OriginalTitle,
            ReleaseDate = tmdbMovie.ReleaseDate,
            RunTime = tmdbMovie.RunTime,
            Title = tmdbMovie.Title,
            WatchedDate = null,
            LastUpdate = DateTime.UtcNow,
            ETag = tmdbMovie.ETag
        };

        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        return new SuccessResult<string>(movie.Title);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        LogMessage.MovieDelete(_logger, id);

        MovieEF? movie = await _dbContext.Movies.Where(m => m.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);
        if (movie is null)
        {
            LogMessage.MovieDeleteFailed(_logger, id, "Movie not found");
            return new NotFoundResult();
        }

        _dbContext.Movies.Remove(movie);        
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);

        return new SuccessResult();
    }

    public async Task<IList<Movie>> GetAllAsync()
    {
        List<MovieEF> dbMovies = await _dbContext.Movies.OrderBy(m => m.Title).ToListAsync();

        var result = new List<Movie>(dbMovies.Count);
        foreach (MovieEF movie in dbMovies)
        {
            result.Add(ConvertToMovie(movie));
        }

        return result;
    }

    public async Task<IList<Movie>> GetWatchListAsync()
    {
        List<MovieEF> dbMovies = await _dbContext.Movies.Where(m => !m.WatchedDate.HasValue).OrderBy(m => m.Title).ToListAsync();

        var result = new List<Movie>(dbMovies.Count);
        foreach (MovieEF movie in dbMovies)
        {
            result.Add(ConvertToMovie(movie));
        }

        return result;
    }

    public async Task<Result<Movie>> GetAsync(int id)
    {
        MovieEF? dbMovie = await _dbContext.Movies.Where(m => m.Id == id).SingleOrDefaultAsync();

        return dbMovie is null
            ? new NotFoundResult<Movie>()
            : new SuccessResult<Movie>(ConvertToMovie(dbMovie));
    }

    public async Task RefreshAsync()
    {
        List<MovieEF> movies = await _dbContext.Movies
            .AsTracking()
            .OrderBy(m => m.WatchedDate == null)
            .ThenByDescending(m => m.Id)
            .ToListAsync();

        var updateCount = 0;
        foreach (MovieEF movie in movies)
        {
            if (updateCount >= 10)
            {
                return;
            }

            if (movie.LastUpdate.AddYears(1) < DateTime.UtcNow)
            {
                await RefreshMovieFromTMDBAsync(movie).ConfigureAwait(false);

                updateCount += 1;
            }
        }
    }

    public async Task<Result> UpdateAsync(Movie movie)
    {
        LogMessage.MovieUpdate(_logger, movie);

        MovieEF? existingMovie = await _dbContext.Movies.Where(m => m.Id == movie.Id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

        if (existingMovie is null)
        {
            LogMessage.MovieUpdateFailed(_logger, movie, "Movie not found");
            return new NotFoundResult();
        }

        existingMovie.WatchedDate = movie.WatchedDate;
        existingMovie.Title = movie.Title;

        _dbContext.Movies.Update(existingMovie);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);

        return new SuccessResult();
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

    private async Task DownloadCoverAsync(int id, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var url = string.Empty;

        try
        {
            var filename = Path.Combine(_configuration["ImagePath"], "movies", $"{id}.jpg");

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

    private async Task RefreshMovieFromTMDBAsync(MovieEF movie)
    {
        var filename = Path.Combine(_configuration["ImagePath"], "movies", $"{movie.Id}.jpg");

        var etag = movie.ETag;
        if (!_fileService.FileExists(filename))
        {
            etag = null;
        }

        TMDbMovie? tmdbMovie;
        try
        {
            tmdbMovie = await _tmdbService.GetMovieAsync(movie.Id.ToString(), etag).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            LogMessage.TheMovieDbFailed(_logger);
            return;
        }

        if (tmdbMovie is null)
        {
            LogMessage.MovieNotChanged(_logger, movie);

            movie.LastUpdate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return;
        }

        movie.ETag = tmdbMovie.ETag;
        movie.OriginalTitle = tmdbMovie.OriginalTitle;
        movie.ReleaseDate = tmdbMovie.ReleaseDate;
        movie.RunTime = tmdbMovie.RunTime;
        movie.LastUpdate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);

        var url = await _tmdbService.GetImageUrlAsync(tmdbMovie.PosterPath);
        if (!string.IsNullOrWhiteSpace(url))
        {
            await _fileService.DownloadImageAsync(url, filename);
        }
    }  
}
