using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Episodes;

public partial class EpisodeView : ComponentBase
{
    [Inject]
    private IEpisodeManager EpisodeManager { get; set; } = null!;

    [CascadingParameter]
    public ShowPage Parent { get; set; } = null!;

    [Parameter]
    public Show Show { get; set; } = null!;

    [Parameter]
    public Episode Episode { get; set; } = null!;

    public bool IsTaskRunning { get; set; } = false;

    public async Task WatchEpisodeAsync()
    {
        IsTaskRunning = true;

        var data = new ShowWatchData()
        {
            EpisodeId = Episode.Id,
            Watched = true,
        };

        Result result = await EpisodeManager.WatchAsync(data);

        IsTaskRunning = false;
        if (result.Success)
        {
            foreach (Season season in Show.Seasons)
            {
                foreach (Episode episode in season.Episodes)
                {
                    if (episode.Id == Episode.Id)
                    {
                        episode.Watched = true;

                        Show.Calculate();

                        Parent?.Refresh();

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

        Result result = await EpisodeManager.WatchAsync(data);

        IsTaskRunning = false;

        if (result.Success)
        {
            foreach (Season season in Show.Seasons)
            {
                foreach (Episode episode in season.Episodes)
                {
                    if (episode.Id == Episode.Id)
                    {
                        episode.Watched = false;

                        Show.Calculate();

                        Parent?.Refresh();

                        return;
                    }
                }
            }
        }
    }
}
