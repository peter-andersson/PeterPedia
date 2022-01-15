using Microsoft.AspNetCore.Components;
using PeterPedia.Shared;
using PeterPedia.Client.Services;

namespace PeterPedia.Client.Pages.Episodes;

public partial class Episodes : ComponentBase
{
    [Inject]
    private TVService TVService { get; set; } = null!;

    public List<Show> Shows { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await TVService.FetchData();

        Shows = TVService.Shows.Where(s => s.UnwatchedEpisodeCount > 0).OrderBy(m => m.Title).ToList();

        TVService.RefreshRequested += Refresh;
    }

    private void Refresh()
    {
        StateHasChanged();
    }
}