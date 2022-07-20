using Microsoft.AspNetCore.Components;

namespace Episodes.App.Shared;

public partial class SeasonView : ComponentBase
{
    [Inject]
    private ITVService Service { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    [Parameter]
    public TVShow TVShow { get; set; } = null!;

    [Parameter]
    public Season Season { get; set; } = null!;

    [Parameter]
    public bool ShowAll { get; set; }

    private bool IsTaskRunning { get; set; } = false;

    private async Task WatchAsync()
    {
        IsTaskRunning = true;

        foreach (Episode episode in Season.Episodes)
        {
            episode.Watched = true;
        }

        Result result = await Service.UpdateAsync(TVShow);
        if (result.Success)
        {
            ToastService.ShowSuccess("Season marked as watched.");
        }
        else
        {
            ToastService.ShowError(result.ErrorMessage);
        }

        IsTaskRunning = false;
    }

    private async Task UnwatchAsync()
    {
        IsTaskRunning = true;

        foreach (Episode episode in Season.Episodes)
        {
            episode.Watched = false;
        }

        Result result = await Service.UpdateAsync(TVShow);
        if (result.Success)
        {
            ToastService.ShowSuccess("Season marked as unwatched.");
        }
        else
        {
            ToastService.ShowError(result.ErrorMessage);
        }

        IsTaskRunning = false;
    }
}
