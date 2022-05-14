using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PeterPedia.Pages.Episodes;

public partial class AllShows : ComponentBase
{
    [Inject]
    private IEpisodeManager EpisodeManager { get; set; } = null!;

    public List<Show> ShowList { get; set; } = new();

    public string Filter { get; set; } = string.Empty;

    public List<Show> FilterShowList { get; set; } = new();

    public bool NotFound { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        Result<IList<Show>> result = await EpisodeManager.GetAllAsync();

        ShowList.Clear();
        if (result.Success)
        {
            ShowList.AddRange(result.Data);
        }
    }

    public void InputKeyDown(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            FilterShowList.Clear();
            if (!string.IsNullOrWhiteSpace(Filter))
            { 
                FilterShowList.AddRange(ShowList.Where(m => m.Search(Filter)).ToList());
            } 
            
            NotFound = FilterShowList.Count == 0;
        }
    }
}
