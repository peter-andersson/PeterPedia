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

    private async Task ToggleStateAsync()
    {
        IsTaskRunning = true;

        bool state = !Season.IsAllWatched;

        foreach (Episode episode in Season.Episodes)
        {
            episode.Watched = state;
        }

        Result result = await Service.UpdateAsync(TVShow);
        if (result.Success)
        {
            if (state)
            {
                ToastService.ShowSuccess("Season marked as watched.");
            }
            else
            {
                ToastService.ShowSuccess("Season marked as unwatched.");
            }
        }
        else
        {
            ToastService.ShowError(result.ErrorMessage);
        }

        IsTaskRunning = false;
    }
}
