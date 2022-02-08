using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Pages.Episodes;

public partial class ShowEpisodes : ComponentBase
{
    [Parameter]
    public string? Id { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnClose { get; set; }

    [Parameter, AllowNull]
    public Show? Show { get; set; }

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

    //protected override async Task OnInitializedAsync()
    //{    
    //    TVService.RefreshRequested += Refresh;
    //}

    //private void Refresh()
    //{
    //    StateHasChanged();
    //}
}