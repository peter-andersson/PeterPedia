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

    private async Task ToggleStateAsync()
    {
        IsTaskRunning = true;

        Episode.Watched = !Episode.Watched;

        Result result = await Service.UpdateAsync(TVShow);
        if (result.Success)
        {
            if (Episode.Watched)
            {
                ToastService.ShowSuccess("Episode marked as watched.");
            }
            else
            {
                ToastService.ShowSuccess("Episode marked as unwatched.");
            }
        }
        else
        {
            ToastService.ShowError(result.ErrorMessage);
        }

        IsTaskRunning = false;
    }
}
