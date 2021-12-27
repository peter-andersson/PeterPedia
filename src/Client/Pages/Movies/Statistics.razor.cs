using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;

namespace PeterPedia.Client.Pages.Movies;

public partial class Statistics : ComponentBase
{
    [Inject]
    private MovieService MovieService { get; set; } = null!;

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