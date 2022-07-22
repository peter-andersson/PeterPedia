using Microsoft.AspNetCore.Components;

namespace Episodes.App.Shared;

public partial class EpisodeView : ComponentBase
{
    [Inject]
    private ITVService Service { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    [Parameter]
    public TVShow TVShow { get; set; } = null!;

    [Parameter]
    public Episode Episode { get; set; } = null!;

    private bool IsTaskRunning { get; set; } = false;

    private async Task WatchAsync()
    {
        IsTaskRunning = true;

        Episode.Watched = true;

        Result result = await Service.UpdateAsync(TVShow);
        if (result.Success)
        {
            ToastService.ShowSuccess("Episode marked as watched.");
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

        Episode.Watched = false;

        Result result = await Service.UpdateAsync(TVShow);
        if (result.Success)
        {
            ToastService.ShowSuccess("Episode marked as unwatched.");
        }
        else
        {
            ToastService.ShowError(result.ErrorMessage);
        }

        IsTaskRunning = false;
    }
}
