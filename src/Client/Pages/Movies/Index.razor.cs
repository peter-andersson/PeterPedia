using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PeterPedia.Client.Pages.Movies;

public partial class Index : ComponentBase
{
    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    private IJSObjectReference _module = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/movies.js");
    }

    public async Task OpenAddDialog()
    {
        if (_module is not null)
        {
            await _module.InvokeVoidAsync("ShowAddModal");
        }
    }

    public async Task DialogClose()
    {
        if (_module is not null)
        {
            await _module.InvokeVoidAsync("HideAddModal");
        }
    }

    public async Task DialogSuccess()
    {
        if (_module is not null)
        {
            await _module.InvokeVoidAsync("HideAddModal");
        }

        StateHasChanged();
    }
}