using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.TV;

public partial class Episodes : ComponentBase
{
    [Inject]
    private ITVShows TVShows { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    public List<Show> WatchList { get; set; } = new List<Show>();

    protected override async Task OnInitializedAsync()
    {
        Result<IList<Show>> result = await TVShows.GetWatchlistAsync();

        WatchList.Clear();

        if (result is SuccessResult<IList<Show>> successResult)
        {
            
            WatchList.AddRange(successResult.Data);
        }
    }

    public void OpenShow(Show show) => Navigation.NavigateTo($"/tv/{show.Id}");
}
