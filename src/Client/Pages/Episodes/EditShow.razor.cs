using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Pages.Episodes;

public partial class EditShow : ComponentBase
{
    [Inject]
    private TVService TVService { get; set; } = null!;

    [Parameter]
    public string? Id { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnClose { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnSuccess { get; set; }

    [Parameter, AllowNull]
    public Show? Show { get; set; }

    public bool IsTaskRunning { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        IsTaskRunning = false;

        if (Show is not null)
        {
            Show = CreateCopy(Show);
        }
    }

    private async Task Save()
    {
        if (Show is null)
        {
            await OnClose.InvokeAsync();
            return;
        }

        IsTaskRunning = true;

        var result = await TVService.Update(Show);

        IsTaskRunning = false;
        if (result)
        {
            await OnSuccess.InvokeAsync();
        }
    }

    private async Task Cancel()
    {
        await OnClose.InvokeAsync();
    }

    private static Show CreateCopy(Show show)
    {
        var showCopy = new Show
        {
            Id = show.Id,
            Title = show.Title,
            Status = show.Status,
            TheMovieDbUrl = show.TheMovieDbUrl,
        };

        foreach (var season in show.Seasons)
        {
            var seasonCopy = new Season()
            {
                Id = season.Id,
                SeasonNumber = season.SeasonNumber,
            };
            showCopy.Seasons.Add(seasonCopy);

            foreach (var episode in season.Episodes)
            {
                var episodeCopy = new Episode()
                {
                    Id = episode.Id,
                    AirDate = episode.AirDate,
                    EpisodeNumber = episode.EpisodeNumber,
                    Title = episode.Title,
                    Watched = episode.Watched,
                };

                seasonCopy.Episodes.Add(episodeCopy);
            }
        }

        showCopy.Calculate();
        return showCopy;
    }
}