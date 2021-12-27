using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;

namespace PeterPedia.Client.Pages.Movies;

public partial class Index : ComponentBase
{
    [Inject]
    private MovieService MovieService { get; set; } = null!;

    private List<PeterPedia.Shared.Movie> Movies = null!;

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