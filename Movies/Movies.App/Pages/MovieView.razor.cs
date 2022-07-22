using Microsoft.AspNetCore.Components;

namespace Movies.App.Pages;

public partial class MovieView : ComponentBase
{
    [Inject]
    private Navigation Navigation { get; set; } = null!;

    [Parameter]
    public Movie Movie { get; set; } = null!;

    private void SelectMovie(Movie movie) => Navigation.NavigateTo($"/edit/{movie.Id}");
}
