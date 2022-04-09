using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Episodes;

public partial class EpisodeView : ComponentBase, IDisposable
{
    [Inject]
    private IEpisodeManager EpisodeManager { get; set; } = null!;

    [CascadingParameter]
    public Show Show { get; set; } = null!;

    [CascadingParameter]
    public Episode Episode { get; set; } = null!;

    public bool IsTaskRunning { get; set; } = false;

    protected override void OnInitialized() => EpisodeManager.EpisodeChanged += () => RefreshEpisodes();

    private void RefreshEpisodes() => StateHasChanged();

    public async Task WatchEpisodeAsync()
    {
        IsTaskRunning = true;

        var data = new ShowWatchData()
        {
            EpisodeId = Episode.Id,
            Watched = true,
        };

        var result = await EpisodeManager.WatchAsync(Show.Id, data);

        IsTaskRunning = false;
        if (result)
        {
            foreach (Season season in Show.Seasons)
            {
                foreach (Episode episode in season.Episodes)
                {
                    if (episode.Id == Episode.Id)
                    {
                        episode.Watched = true;

                        Show.Calculate();

                        return;
                    }
                }
            }
        }
    }

    public async Task UnwatchEpisodeAsync()
    {
        IsTaskRunning = true;

        var data = new ShowWatchData()
        {
            EpisodeId = Episode.Id,
            Watched = false,
        };

        var result = await EpisodeManager.WatchAsync(Show.Id, data);

        IsTaskRunning = false;

        if (result)
        {
            foreach (Season season in Show.Seasons)
            {
                foreach (Episode episode in season.Episodes)
                {
                    if (episode.Id == Episode.Id)
                    {
                        episode.Watched = false;

                        Show.Calculate();

                        return;
                    }
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
