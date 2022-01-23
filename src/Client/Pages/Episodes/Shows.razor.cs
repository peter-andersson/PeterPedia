using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;
using PeterPedia.Shared;

namespace PeterPedia.Client.Pages.Episodes;

public partial class Shows : ComponentBase
{
    [Inject]
    private TVService TVService { get; set; } = null!;
    
    public List<Show> ShowList { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await TVService.FetchData();

        FilterShows(string.Empty);
    }

    public void FilterShows(string filter)
    {
        IEnumerable<Show> shows = TVService.Shows;

        if (!string.IsNullOrWhiteSpace(filter))
        {
            shows = shows.Where(s => s.Title.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
        }

        ShowList = shows.OrderBy(s => s.Title).ToList();
    }
}