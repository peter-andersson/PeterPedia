using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;

namespace PeterPedia.Client.Pages.Movies;

public partial class Edit : ComponentBase
{
    [Inject]
    private MovieService MovieService { get; set; } = null!;

    [Inject]
    private NavigationManager NavManager { get; set; } = null!;

    [Parameter]
    public int Id { get; set; }

    [Parameter]
    public string ReturnUrl { get; set; } = null!;

    private bool IsTaskRunning;
    private PeterPedia.Shared.Movie Movie = null!;

    protected override async Task OnInitializedAsync()
    {
        ReturnUrl ??= "";

        IsTaskRunning = false;
        var movie = await MovieService.Get(Id);

        if (movie is null)
        {
            NavManager.NavigateTo(ReturnUrl);
        }
        else
        {
            Movie = CreateCopy(movie);
        }
    }

    private async Task Save()
    {
        IsTaskRunning = true;

        var result = await MovieService.Update(Movie);

        IsTaskRunning = false;
        if (result)
        {
            NavManager.NavigateTo(ReturnUrl);
        }
    }

    private void Cancel()
    {
        NavManager.NavigateTo(ReturnUrl);
    }

    private static PeterPedia.Shared.Movie CreateCopy(PeterPedia.Shared.Movie? movie)
    {
        if (movie is null)
        {
            throw new ArgumentNullException(nameof(movie));
        }

        var result = new PeterPedia.Shared.Movie()
        {
            Id = movie.Id,
            Title = movie.Title,
            ImdbUrl = movie.ImdbUrl,
            OriginalLanguage = movie.OriginalLanguage,
            OriginalTitle = movie.OriginalTitle,
            ReleaseDate = movie.ReleaseDate,
            RunTime = movie.RunTime,
            TheMovieDbUrl = movie.TheMovieDbUrl,
            WatchedDate = movie.WatchedDate,
        };

        return result;
    }
}