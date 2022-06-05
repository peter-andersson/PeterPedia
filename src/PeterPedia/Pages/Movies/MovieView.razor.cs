using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Movies;

public partial class MovieView : ComponentBase
{
    [Inject]
    private Navigation Navigation { get; set; } = null!;

    [Parameter]
    public Movie Movie { get; set; } = null!;

    private void SelectMovie(Movie movie) => Navigation.NavigateTo($"/movies/edit/{movie.Id}");
}
