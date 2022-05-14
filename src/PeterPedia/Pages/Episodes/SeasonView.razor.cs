using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Episodes;

public partial class SeasonView : ComponentBase
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

    public async Task WatchSeasonAsync()
    {
        var data = new ShowWatchData()
        {
            SeasonId = Season.Id,
            Watched = true,
        };

        IsTaskRunning = true;
        Result result = await EpisodeManager.WatchAsync(data);
        IsTaskRunning = false;

        if (result.Success)
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
        Result result = await EpisodeManager.WatchAsync(data);
        IsTaskRunning = false;

        if (result.Success)
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
}
