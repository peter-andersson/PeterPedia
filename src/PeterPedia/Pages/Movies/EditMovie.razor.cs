using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Movies;

public partial class EditMovie : ComponentBase
{
    [Inject]
    private IMovieManager MovieManager { get; set; } = null!;

    [Inject]
    private NavigationManager NavManager { get; set; } = null!;

    [Parameter]
    public int Id { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public Movie Movie { get; set; } = new Movie();

    public bool IsTaskRunning { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsTaskRunning = false;
        ErrorMessage = string.Empty;

        Result<Movie> result = await MovieManager.GetAsync(Id);

        if (result is SuccessResult<Movie> successResult)
        {
            Movie = successResult.Data;
        }
        else
        {
            ErrorMessage = "Movie not found";
        }
    }

    public async Task SaveAsync()
    {
        ErrorMessage = string.Empty;
        IsTaskRunning = true;

        Result result = await MovieManager.UpdateAsync(Movie);

        if (result.Success)
        {
            NavManager.NavigateTo("movies");
        }
        else
        {
            ErrorMessage = "Failed to update movies.";
        }

        IsTaskRunning = false;
    }

    private async Task DeleteAsync()
    {
        ErrorMessage = string.Empty;
        IsTaskRunning = true;

        Result result = await MovieManager.DeleteAsync(Movie.Id);

        if (result.Success)
        {
            NavManager.NavigateTo("movies");
        }
        else
        {
            ErrorMessage = "Failed to delete movies.";
        } 

        IsTaskRunning = false;
    }

    private void Close() => NavManager.NavigateTo("movies");
}
