using System.ComponentModel.DataAnnotations;

namespace Movies.App.Models
{
    public class EditMovie
    {

        public EditMovie(Movie movie)
        {
            Id = movie.Id;
            ImdbId = movie.ImdbId;
            OriginalTitle = movie.OriginalTitle;
            OriginalLanguage = movie.OriginalLanguage;
            Title = movie.Title;
            ReleaseDate = movie.ReleaseDate;
            WatchedDate = movie.WatchedDate;
            RunTime = movie.RunTime;
            Refresh = false;
            TheMovieDbUrl = $"https://www.themoviedb.org/movie/{Id}";
            ImdbUrl = $"https://www.imdb.com/title/{ImdbId}";
        }

        public string Id { get; private set; }

        public string? ImdbId { get; private set; }

        public string? OriginalTitle { get; private set; }

        public string? OriginalLanguage { get; private set; }

        [Required]
        public string Title { get; set; }

        public DateTime? ReleaseDate { get; private set; }

        public DateTime? WatchedDate { get; set; }

        public int? RunTime { get; private set; }

        public bool Refresh { get; set; }

        public string TheMovieDbUrl { get; private set; }

        public string ImdbUrl { get; private set; }
    }
}
