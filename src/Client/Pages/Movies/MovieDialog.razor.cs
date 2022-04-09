using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Movies;

public partial class MovieDialog : ComponentBase
{
    [Inject]
    private IMovieManager MovieManager { get; set; } = null!;

    [CascadingParameter]
    private BlazoredModalInstance ModalInstance { get; set; } = null!;

    [Parameter]
    public Movie Movie { get; set; } = null!;

    public MovieModel EditMovie { get; set; } = new MovieModel();

    public bool IsTaskRunning { get; set; }

    protected override void OnInitialized()
    {
        IsTaskRunning = false;

        EditMovie.Title = Movie.Title;
        EditMovie.WatchedDate = Movie.WatchedDate;
    }

    public async Task SaveAsync()
    {       
        IsTaskRunning = true;

        if (!string.IsNullOrWhiteSpace(EditMovie.Title))
        {
            Movie.Title = EditMovie.Title;
        }

        Movie.WatchedDate = EditMovie.WatchedDate;

        var result = await MovieManager.UpdateAsync(Movie);

        IsTaskRunning = false;
        if (result)
        {
            await ModalInstance.CancelAsync();
        }
    }

    private async Task DeleteAsync()
    {
        IsTaskRunning = true;

        if (await MovieManager.DeleteAsync(Movie.Id))
        {
            await ModalInstance.CloseAsync();
        }

        IsTaskRunning = false;
    }

    private async Task CloseAsync() => await ModalInstance.CancelAsync();   
}
