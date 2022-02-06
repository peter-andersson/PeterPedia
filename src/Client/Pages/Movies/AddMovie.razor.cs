using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Pages.Movies;

public partial class AddMovie : ComponentBase
{
    [Inject]
    private MovieService MovieService { get; set; } = null!;

    [Parameter]
    public string? Id { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnClose { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnSuccess { get; set; }

    public bool IsTaskRunning { get; set; } = false;

    public string MovieUrl { get; set; } = string.Empty;

    public async Task InputKeyDown(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await AddMovieButton();
        }

    }

    public async Task AddMovieButton()
    {
        if (string.IsNullOrEmpty(MovieUrl))
        {
            return;
        }

        IsTaskRunning = true;

        var result = await MovieService.Add(MovieUrl);
        IsTaskRunning = false;

        if (result)
        {
            MovieUrl = string.Empty;

            await OnSuccess.InvokeAsync();
        }
    }

    public async Task Close()
    {
        await OnClose.InvokeAsync();
    }
}