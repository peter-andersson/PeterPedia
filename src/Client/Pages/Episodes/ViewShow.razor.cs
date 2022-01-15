using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;
using PeterPedia.Shared;

namespace PeterPedia.Client.Pages.Episodes;

public partial class ViewShow : ComponentBase
{
    [Inject]
    private TVService TVService { get; set; } = null!;

    [Parameter]
    public int Id { get; set; }

    private Show? Show;

    protected override async Task OnInitializedAsync()
    {
        await TVService.FetchData();

        Show = await TVService.Get(Id);

        TVService.RefreshRequested += Refresh;
    }
    private void Refresh()
    {
        StateHasChanged();
    }
}