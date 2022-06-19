using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.TV;

public partial class ShowPage : ComponentBase
{
    [Inject]
    private ITVShows TVShows { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    [Parameter]
    public int Id { get; set; }

    public Show Show { get; set; } = new Show();

    public bool ShowAll { get; set; }

    public void ToggleShowAll() => ShowAll = !ShowAll;

    public bool IsTaskRunning { get; set; }

    public void Refresh() => StateHasChanged();

    protected override async Task OnInitializedAsync()
    {
        IsTaskRunning = false;

        Result<Show> result = await TVShows.GetAsync(Id);

        if (result.Success)
        {
            Show = result.Data;
        }
    }

    public async Task SaveAsync()
    {
        IsTaskRunning = true;

        Result<Show> result = await TVShows.UpdateAsync(Show);

        IsTaskRunning = false;
        if (result.Success)
        {
            Navigation.NavigateBack();
        }
    }

    public async Task DeleteAsync()
    {
        IsTaskRunning = true;

        Result result = await TVShows.DeleteAsync(Show.Id);

        if (result.Success)
        {
            Navigation.NavigateBack();
        }

        IsTaskRunning = false;
    }

    public void Close() => Navigation.NavigateBack();

    public async Task WatchSeasonAsync(Season season)
    {
        var data = new ShowWatchData()
        {
            SeasonId = season.Id,
            Watched = true,
        };

        IsTaskRunning = true;
        Result result = await TVShows.WatchAsync(data);
        IsTaskRunning = false;

        if (result.Success)
        {
            foreach (Episode episode in season.Episodes)
            {
                episode.Watched = true;
            }

            Show.Calculate();
        }
    }

    public async Task UnwatchSeasonAsync(Season season)
    {
        var data = new ShowWatchData()
        {
            SeasonId = season.Id,
            Watched = false,
        };

        IsTaskRunning = true;
        Result result = await TVShows.WatchAsync(data);
        IsTaskRunning = false;

        if (result.Success)
        {
            foreach (Episode episode in season.Episodes)
            {
                episode.Watched = false;
            }

            Show.Calculate();
        }
    }

    public async Task WatchEpisodeAsync(Episode episode)
    {
        IsTaskRunning = true;

        var data = new ShowWatchData()
        {
            EpisodeId = episode.Id,
            Watched = true,
        };

        Result result = await TVShows.WatchAsync(data);

        IsTaskRunning = false;
        if (result.Success)
        {
            episode.Watched = true;

            Show.Calculate();
        }
    }

    public async Task UnwatchEpisodeAsync(Episode episode)
    {
        IsTaskRunning = true;

        var data = new ShowWatchData()
        {
            EpisodeId = episode.Id,
            Watched = false,
        };

        Result result = await TVShows.WatchAsync(data);

        IsTaskRunning = false;

        if (result.Success)
        {
            episode.Watched = false;

            Show.Calculate();
        }
    }
}
