namespace PeterPedia.Client.Movie.Pages
{
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using PeterPedia.Client.Movie.Services;
    using PeterPedia.Shared;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class Index : ComponentBase
    {
        [Inject]
        private MovieService MovieService { get; set; }

        private List<Movie> Movies;

        protected override async Task OnInitializedAsync()
        {
            await MovieService.FetchData();

            LoadWatchList();
        }

        private void LoadWatchList()
        {
            Movies = MovieService.Movies.Where(m => !m.WatchedDate.HasValue).OrderBy(m => m.Title).ToList();
        }
    }
}
