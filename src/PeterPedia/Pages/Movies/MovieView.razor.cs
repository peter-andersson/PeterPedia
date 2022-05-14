using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Movies;

public partial class MovieView : ComponentBase
{
    [Inject]
    private NavigationManager NavManager { get; set; } = null!;

    [Parameter]
    public Movie Movie { get; set; } = null!;

    private void SelectMovie(Movie movie) => NavManager.NavigateTo($"/movies/edit/{movie.Id}");
}
