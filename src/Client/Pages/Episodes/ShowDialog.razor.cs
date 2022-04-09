using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Episodes;

public partial class ShowDialog : ComponentBase
{
    [Inject]
    private IEpisodeManager EpisodeManager { get; set; } = null!;

    [CascadingParameter]
    private BlazoredModalInstance ModalInstance { get; set; } = null!;

    [Parameter]
    public Show Show { get; set; } = null!;

    public ShowModel EditShow { get; set; } = new ShowModel();

    public bool ShowAll { get; set; }

    public void ToggleShowAll() => ShowAll = !ShowAll;

    public bool IsTaskRunning { get; set; }

    protected override void OnInitialized()
    {
        IsTaskRunning = false;

        EditShow.Title = Show.Title;
    }

    public async Task SaveAsync()
    {       
        IsTaskRunning = true;

        if (!string.IsNullOrWhiteSpace(EditShow.Title))
        {
            Show.Title = EditShow.Title;
        }

        Show.ForceRefresh = EditShow.Refresh;

        var result = await EpisodeManager.UpdateAsync(Show);

        IsTaskRunning = false;
        if (result)
        {
            await ModalInstance.CancelAsync();
        }
    }

    private async Task DeleteAsync()
    {
        IsTaskRunning = true;

        if (await EpisodeManager.DeleteAsync(Show.Id))
        {
            await ModalInstance.CloseAsync();
        }

        IsTaskRunning = false;
    }

    private async Task CloseAsync() => await ModalInstance.CancelAsync();   
}
