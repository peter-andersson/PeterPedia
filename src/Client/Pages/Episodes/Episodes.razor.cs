using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PeterPedia.Client.Pages.Episodes;

public partial class Episodes : ComponentBase
{
    private IJSObjectReference _module = null!;

    [Inject]
    private TVService TVService { get; set; } = null!;

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    public List<Show> ShowList { get; set; } = null!;

    public bool Unwatched { get; set; } = true;

    public Show? SelectedShow { get; set; } = null;

    public string AddShowElement { get; } = "add-show-dialog";

    public string DeleteShowElement { get; } = "delete-show-dialog";

    public string EditShowElement { get; } = "edit-show-dialog";

    public string ShowEpisodeElement { get; } = "show-episode-dialog";

    private string _currentFilter = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await FilterShows(string.Empty);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/dialog.js");
    }

    public async Task ToggleUnwatched()
    {
        Unwatched = !Unwatched;

        await FilterShows(string.Empty);
    }

    public async Task FilterShows(string filter)
    {
        _currentFilter = filter;

        await TVService.FetchDataAsync();

        IEnumerable<Show> shows;

        if (Unwatched)
        {
            shows = TVService.Shows.Where(s => s.UnwatchedEpisodeCount > 0);
        }
        else
        {
            shows = TVService.Shows;
        }

        if (!string.IsNullOrWhiteSpace(_currentFilter))
        {
            shows = shows.Where(s => s.Title.Contains(_currentFilter, StringComparison.OrdinalIgnoreCase));
        }

        ShowList = shows.OrderBy(s => s.Title).ToList();
    }

    public async Task AddShow()
    {
        await ShowDialog(AddShowElement);
    }

    public async Task AddDialogClose()
    {
        await HideDialog(AddShowElement);
    }

    public async Task AddDialogSuccess()
    {
        await AddDialogClose();

        await FilterShows(_currentFilter);

        StateHasChanged();
    }

    public async Task DeleteShow(Show show)
    {
        SelectedShow = show;        

        await ShowDialog(DeleteShowElement);
    }

    public async Task DeleteDialogClose()
    {
        SelectedShow = null;

        await HideDialog(DeleteShowElement);
    }

    public async Task DeleteDialogSuccess()
    {
        await DeleteDialogClose();

        await FilterShows(_currentFilter);

        StateHasChanged();
    }

    public async Task EditShow(Show show)
    {
        SelectedShow = show;

        await ShowDialog(EditShowElement);
    }

    public async Task EditDialogClose()
    {
        await HideDialog(EditShowElement);
    }

    public async Task EditDialogSuccess()
    {
        await EditDialogClose();

        await FilterShows(_currentFilter);

        StateHasChanged();
    }

    public async Task ShowEpisodes(Show show)
    {
        SelectedShow = show;

        await ShowDialog(ShowEpisodeElement);
    }

    public async Task EpisodeDialogClose()
    {
        await HideDialog(ShowEpisodeElement);

        await FilterShows(_currentFilter);

        StateHasChanged();
    }
    
    private async Task ShowDialog(string element)
    {
        if (string.IsNullOrWhiteSpace(element))
        {
            return;
        }

        if (_module is not null)
        {
            await _module.InvokeVoidAsync("ShowDialog", element);
        }
    }

    private async Task HideDialog(string element)
    {
        if (string.IsNullOrWhiteSpace(element))
        {
            return;
        }

        if (_module is not null)
        {
            await _module.InvokeVoidAsync("HideDialog", element);
        }
    }
}