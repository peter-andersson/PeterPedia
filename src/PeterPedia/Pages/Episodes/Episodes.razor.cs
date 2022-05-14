using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Episodes;

public partial class Episodes : ComponentBase
{
    [Inject]
    private IEpisodeManager EpisodeManager { get; set; } = null!;

    public List<Show> WatchList { get; set; } = new List<Show>();

    protected override async Task OnInitializedAsync()
    {
        Result<IList<Show>> result = await EpisodeManager.GetWatchlistAsync();

        WatchList.Clear();

        if (result is SuccessResult<IList<Show>> successResult)
        {
            
            WatchList.AddRange(successResult.Data);
        }
    }      
}
