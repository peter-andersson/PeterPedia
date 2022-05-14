using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Movies;

public partial class Movies : ComponentBase
{
    [Inject]
    private IMovieManager MovieManager { get; set; } = null!;

    public List<Movie> MovieList { get; set; } = new();

    protected override async Task OnInitializedAsync() => MovieList.AddRange(await MovieManager.GetWatchListAsync());
}
