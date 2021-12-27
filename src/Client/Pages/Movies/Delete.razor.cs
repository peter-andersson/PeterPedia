using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;

namespace PeterPedia.Client.Pages.Movies;

public partial class Delete : ComponentBase
{
    [Inject]
    private MovieService MovieService { get; set; } = null!;

    [Inject]
    private NavigationManager NavManager { get; set; } = null!;

    [Parameter]
    public int Id { get; set; }

    [Parameter]
    public string ReturnUrl { get; set; } = null!;

    private string Title =  null!;

    private bool IsTaskRunning;

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
            Title = movie.Title;
        }
    }

    private async Task DeleteMovie()
    {
        IsTaskRunning = true;

        var result = await MovieService.Delete(Id);

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
}