using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Episodes;

public partial class ShowPage : ComponentBase
{
    [Inject]
    private IEpisodeManager EpisodeManager { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    [Parameter]
    public int Id { get; set; }

    public Show Show { get; set; } = new Show();

    public bool ShowAll { get; set; }

    public void ToggleShowAll() => ShowAll = !ShowAll;

    public bool IsTaskRunning { get; set; }

    public void Refresh() => StateHasChanged();

    protected override async Task OnInitializedAsync()
    {
        IsTaskRunning = false;

        Result<Show> result = await EpisodeManager.GetAsync(Id);

        if (result.Success)
        {
            Show = result.Data;
        }
    }

    private async Task SaveAsync()
    {       
        IsTaskRunning = true;

        Result<Show> result = await EpisodeManager.UpdateAsync(Show);

        IsTaskRunning = false;
        if (result.Success)
        {
            Navigation.NavigateBack();
        }
    }    

    private async Task DeleteAsync()
    {
        IsTaskRunning = true;

        Result result = await EpisodeManager.DeleteAsync(Show.Id);

        if (result.Success)
        {
            Navigation.NavigateBack();
        }

        IsTaskRunning = false;
    }

    private void Close() => Navigation.NavigateBack();
}
