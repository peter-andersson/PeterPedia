using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PeterPedia.Client.Services;
using PeterPedia.Shared;

namespace PeterPedia.Client.Pages.Episodes;

public partial class Shows : ComponentBase
{
    private enum NavigatePage
    {
        Next,
        Previous,
        First
    }

    [Inject]
    private TVService TVService { get; set; } = null!;

    private readonly int pageSize = 25;

    private int currentPage = 0;

    private List<Show> DisplayShows = null!;

    private string CurrentSearch = string.Empty;

    private EditContext SearchContext = null!;

    private bool IsPreviousButtonDisabled = false;

    private bool IsNextButtonDisabled = false;

    protected override async Task OnInitializedAsync()
    {
        await TVService.FetchData();

        SearchContext = new EditContext(CurrentSearch);

        LoadDisplayShows();

        StateHasChanged();
    }

    private void NavigateToPage(NavigatePage navigateTo)
    {
        switch (navigateTo)
        {
            case NavigatePage.Next:
                currentPage += 1;
                break;
            case NavigatePage.Previous:
                currentPage -= 1;
                if (currentPage < 0)
                {
                    currentPage = 0;
                }
                break;
            case NavigatePage.First:
                currentPage = 0;
                break;
        }

        LoadDisplayShows();
    }

    private void LoadDisplayShows()
    {
        if (!string.IsNullOrWhiteSpace(CurrentSearch))
        {
            DisplayShows = TVService.Shows.Where(m => m.Title.Contains(CurrentSearch, StringComparison.InvariantCultureIgnoreCase)).OrderBy(m => m.Title).Skip(currentPage * pageSize).Take(pageSize).ToList();
        }
        else
        {
            DisplayShows = TVService.Shows.OrderBy(m => m.Title).Skip(currentPage * pageSize).Take(pageSize).ToList();
        }

        IsPreviousButtonDisabled = currentPage == 0;
        IsNextButtonDisabled = DisplayShows.Count < pageSize;

        StateHasChanged();
    }
}