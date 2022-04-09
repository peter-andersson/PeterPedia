using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Episodes;

public partial class SeasonView : ComponentBase, IDisposable
{
    [Inject]
    private IEpisodeManager EpisodeManager { get; set; } = null!;

    [CascadingParameter]
    public Show Show { get; set; } = null!;

    [CascadingParameter]
    public Season Season { get; set; } = null!;

    [Parameter]
    public bool DisplayAllEpisodes { get; set; }    

    public bool IsTaskRunning { get; set; } = false;

    protected override void OnInitialized() => EpisodeManager.EpisodeChanged += () => RefreshEpisodes();

    private void RefreshEpisodes() => StateHasChanged();

    public async Task WatchSeasonAsync()
    {
        var data = new ShowWatchData()
        {
            SeasonId = Season.Id,
            Watched = true,
        };

        IsTaskRunning = true;
        var result = await EpisodeManager.WatchAsync(Show.Id, data);
        IsTaskRunning = false;

        if (result)
        {
            foreach (Season season in Show.Seasons)
            {
                if (season.Id == Season.Id)
                {
                    foreach (Episode episode in season.Episodes)
                    {
                        episode.Watched = true;
                    }

                    Show.Calculate();

                    return;
                }
            }
        }
    }

    public async Task UnwatchSeasonAsync()
    {
        var data = new ShowWatchData()
        {
            SeasonId = Season.Id,
            Watched = false,
        };

        IsTaskRunning = true;
        var result = await EpisodeManager.WatchAsync(Show.Id, data);
        IsTaskRunning = false;

        if (result)
        {
            foreach (Season season in Show.Seasons)
            {
                if (season.Id == Season.Id)
                {
                    foreach (Episode episode in season.Episodes)
                    {
                        episode.Watched = false;
                    }

                    Show.Calculate();

                    return;
                }
            }
        }
    }

    public void Dispose()
    {
        EpisodeManager.EpisodeChanged -= () => RefreshEpisodes();

        GC.SuppressFinalize(this);
    }
}
