namespace PeterPedia.Client.Movie.Pages
{
    using Microsoft.AspNetCore.Components;
    using PeterPedia.Client.Movie.Services;
    using PeterPedia.Shared;
    using System.Threading.Tasks;

    public partial class Edit : ComponentBase
    {
        [Inject]
        private MovieService MovieService { get; set; }

        [Inject]
        private NavigationManager NavManager { get; set; }

        [Parameter]
        public int Id { get; set; }

        [Parameter]
        public string ReturnUrl { get; set; }

        private bool IsTaskRunning;
        private Movie Movie;

        protected override async Task OnInitializedAsync()
        {
            ReturnUrl = ReturnUrl ?? "/";

            IsTaskRunning = false;
            var movie = await MovieService.Get(Id);

            if (movie is null)
            {
                NavManager.NavigateTo(ReturnUrl);
            }
            else
            {
                Movie = CreateCopy(movie);
            }
        }

        private async Task Save()
        {
            IsTaskRunning = true;

            var result = await MovieService.Update(Movie);

            IsTaskRunning = false;
            if (result)
            {
                NavManager.NavigateTo(ReturnUrl);
            }
        }

        private void Cancel()
        {
            NavManager.NavigateTo(ReturnUrl);
        }

        private static Movie CreateCopy(Movie movie)
        {
            var result = new Movie()
            {
                Id = movie.Id,
                Title = movie.Title,
                ImdbUrl = movie.ImdbUrl,
                OriginalLanguage = movie.OriginalLanguage,
                OriginalTitle = movie.OriginalTitle,
                ReleaseDate = movie.ReleaseDate,
                RunTime = movie.RunTime,
                TheMovieDbUrl = movie.TheMovieDbUrl,
                WatchedDate = movie.WatchedDate,
            };

            return result;
        }
    }
}
