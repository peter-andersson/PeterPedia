using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Pages.Movies;

public partial class DeleteMovie : ComponentBase
{
    [Inject]
    private MovieService MovieService { get; set; } = null!;

    [Parameter]
    public string? Id { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnClose { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnSuccess { get; set; }

    [Parameter, AllowNull]
    public Movie? Movie { get; set; }

    public bool IsTaskRunning { get; set; } = false;

    public async Task Delete()
    {
        if (Movie is null)
        {
            await OnClose.InvokeAsync();
            return;
        }

        IsTaskRunning = true;

        await MovieService.DeleteAsync(Movie.Id);

        IsTaskRunning = false;

        await OnSuccess.InvokeAsync();
    }

    public async Task Cancel()
    {
        await OnClose.InvokeAsync();
    }
}