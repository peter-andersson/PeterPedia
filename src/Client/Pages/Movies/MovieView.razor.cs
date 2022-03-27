using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Movies;

public partial class MovieView : ComponentBase
{
    [Parameter]
    public Movie Movie { get; set; } = null!;

    [Parameter]
    public EventCallback<Movie> OnSelect { get; set; }

    private async Task SelectMovieAsync(Movie movie) => await OnSelect.InvokeAsync(movie);
}
