using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.TV;

public partial class NewEpisodes : ComponentBase
{
    [Inject]
    private ITVShows TVShows { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    public List<Episode> Episodes { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Result<IList<Episode>> result =  await TVShows.GetEpisodesAsync();

        Episodes.Clear();
        if (result.Success)
        {
            Episodes.AddRange(result.Data);
        }
    }

    private void Close() => Navigation.NavigateBack();
}
