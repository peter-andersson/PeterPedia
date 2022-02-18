using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Pages.Episodes;

public partial class ShowEpisodes : ComponentBase
{
    [Inject]
    private TVService TVService { get; set; } = null!;

    [Parameter]
    public string? Id { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnClose { get; set; }

    [Parameter, AllowNull]
    public Show? Show { get; set; }

    protected override void OnInitialized()
    {
        TVService.RefreshRequested += TVService_RefreshRequested;
    }

    private void TVService_RefreshRequested()
    {
        StateHasChanged();
    }

    public string Title
    {
        get
        {
            return $"{Show?.Title} - {Show?.Status}";
        }
    }

    public bool ShowAll { get; set; } = false;

    public async Task Close()
    {
        await OnClose.InvokeAsync();
    }

    public void ToggleShowAll()
    {
        ShowAll = !ShowAll;
    }
}