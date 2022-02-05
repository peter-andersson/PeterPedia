using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Pages.Movies;

public partial class EditMovie : ComponentBase
{
    [Inject]
    private MovieService MovieService { get; set; } = null!;

    [Parameter, AllowNull]
    public EventCallback<string> OnClose { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnSuccess { get; set; }

    [Parameter, AllowNull]
    public Movie? Movie { get; set; }

    public bool IsTaskRunning { get; set; }

    protected override void OnInitialized()
    {
        IsTaskRunning = false;

        if (Movie is not null)
        {
            Movie = CreateCopy(Movie);
        }
    }

    public async Task Save()
    {
        if (Movie is null)
        {
            await OnClose.InvokeAsync();
            return;
        }

        IsTaskRunning = true;

        var result = await MovieService.Update(Movie);

        IsTaskRunning = false;
        if (result)
        {
            await OnSuccess.InvokeAsync();
        }
    }

    public async Task Cancel()
    {
        await OnClose.InvokeAsync();
    }
       
    private static Movie CreateCopy(Movie? movie)
    {
        if (movie is null)
        {
            throw new ArgumentNullException(nameof(movie));
        }

        var result = new Movie()
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