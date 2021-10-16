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

    public partial class Statistics : ComponentBase
    {
        [Inject]
        private MovieService MovieService { get; set; }

        private int Watchlist { get; set; }
        private int Watched { get; set; }
        private int Total
        {
            get
            {
                return Watchlist + Watched;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            Watchlist = 0;
            Watched = 0;

            await MovieService.FetchData();

            CalculateStatisticts();
        }

        private void CalculateStatisticts()
        {
            foreach (var movie in MovieService.Movies)
            {
                if (movie.WatchedDate.HasValue)
                {
                    Watched += 1;
                }
                else
                {
                    Watchlist += 1;
                }
            }
        }
    }
}
