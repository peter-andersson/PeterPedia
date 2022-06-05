using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Episodes;

public partial class NewEpisodes : ComponentBase
{
    [Inject]
    private IEpisodeManager EpisodeManager { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    public List<Episode> Episodes { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Result<IList<Episode>> result =  await EpisodeManager.GetEpisodesAsync();

        Episodes.Clear();
        if (result.Success)
        {
            Episodes.AddRange(result.Data);
        }
    }

    private void Close() => Navigation.NavigateBack();
}
