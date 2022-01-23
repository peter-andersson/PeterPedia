using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;

namespace PeterPedia.Client.Pages.Movies;

public partial class Index : ComponentBase
{
    [Inject]
    private MovieService MovieService { get; set; } = null!;

    public List<PeterPedia.Shared.Movie> MovieList { get; private set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await FilterMovies(string.Empty);
    }

    public async Task FilterMovies(string filter)
    {
        MovieList = await MovieService.GetMovies(filter, watchList: true);
    }
}