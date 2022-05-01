using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Episodes;

public partial class Episodes : ComponentBase, IDisposable
{
    [Inject]
    private IEpisodeManager EpisodeManager { get; set; } = null!;

    [CascadingParameter]
    private IModalService Modal { get; set; } = null!;

    private readonly List<Show> _shows = new();

    public string Filter { get; set; } = string.Empty;

    public bool Loading { get; set; } = true;    

    public List<Show> AllShows => _shows.Where(s => s.Search(Filter)).ToList();

    public List<Show> WatchList => _shows.Where(s => s.UnwatchedEpisodeCount > 0).ToList();    

    protected override async Task OnInitializedAsync()
    {
        EpisodeManager.EpisodeChanged += async () => await RefreshEpisodesAsync();

        await RefreshEpisodesAsync();

        Loading = false;
    }

    private async Task RefreshEpisodesAsync()
    {
        List<Show> shows = await EpisodeManager.GetAsync();

        _shows.Clear();
        _shows.AddRange(shows.OrderBy(a => a.Title).ToList());

        StateHasChanged();
    }

    public void AddShow() => _ = Modal.Show<AddShowDialog>("Add show", new ModalOptions()
    {
        Class = "blazored-modal w-50",
    });

    public void ShowLastEpisodes() => _ = Modal.Show<EpisodesDialog>("Latest episodes", new ModalOptions()
    {
        Class = "blazored-modal w-50 overflow-scroll vh-75 mh-100",
    });

    private void SelectShow(Show show)
    {
        var parameters = new ModalParameters();
        parameters.Add("Show", show);

        Modal.Show<ShowDialog>("Edit show", parameters, new ModalOptions()
        {
            Class = "blazored-modal w-75 overflow-scroll vh-75 mh-100",
        });
    }

    public void Dispose()
    {
        EpisodeManager.EpisodeChanged -= async () => await RefreshEpisodesAsync();

        GC.SuppressFinalize(this);
    }    
}
