using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PeterPedia.Pages.TV;

public partial class AllShows : ComponentBase
{
    [Inject]
    private ITVShows TVShows { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    private List<Show> _showList = new();

    public string Filter { get; set; } = string.Empty;

    public List<Show> ShowList { get; set; } = new();

    public bool NotFound { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        Result<IList<Show>> result = await TVShows.GetAllAsync();

        _showList.Clear();
        if (result.Success)
        {
            _showList.AddRange(result.Data);

            ShowList.AddRange(_showList.OrderBy(s => s.Title).ToList());
        }
    }

    public void InputKeyDown(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            ShowList.Clear();
            if (string.IsNullOrWhiteSpace(Filter))
            {
                ShowList.AddRange(_showList.Where(s => s.Search(Filter)).OrderBy(s => s.Title).ToList());
            } 
            
            NotFound = ShowList.Count == 0;
        }
    }

    public void OpenShow(Show show) => Navigation.NavigateTo($"/tv/{show.Id}");
}
