using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;
using PeterPedia.Shared;

namespace PeterPedia.Client.Pages.Episodes;

public partial class SeasonView : ComponentBase
{
    [Inject]
    private TVService TVService { get; set; } = null!;

    [CascadingParameter]
    public Show Show { get; set; } = null!;

    [CascadingParameter]
    public Season Season { get; set; } = null!;

    [Parameter]
    public bool DisplayAllEpisodes { get; set; }    

    private bool IsTaskRunning = false;

    public async Task WatchSeason()
    {
        IsTaskRunning = true;
        var result = await TVService.WatchSeason(Show.Id, Season.Id);
        IsTaskRunning = false;

        if (result)
        {
            foreach (var season in Show.Seasons)
            {
                if (season.Id == Season.Id)
                {
                    foreach (var episode in season.Episodes)
                    {
                        episode.Watched = true;
                    }

                    Show.Calculate();
                    TVService.CallRequestRefresh();

                    return;
                }
            }
        }
    }

    public async Task UnwatchSeason()
    {
        IsTaskRunning = true;
        var result = await TVService.UnwatchSeason(Show.Id, Season.Id);
        IsTaskRunning = false;

        if (result)
        {
            foreach (var season in Show.Seasons)
            {
                if (season.Id == Season.Id)
                {
                    foreach (var episode in season.Episodes)
                    {
                        episode.Watched = false;
                    }

                    Show.Calculate();
                    TVService.CallRequestRefresh();

                    return;
                }
            }
        }
    }
}