using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Videos;

public partial class Videos : ComponentBase
{
    [Inject]
    public VideoService VideoService { get; set; } = null!;

    private readonly List<Video> _videos = new();

    public List<Video> VideoList
    {
        get
        {
            IEnumerable<Video> query = _videos;

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                query = query.Where(v => v.Title.Contains(Filter, StringComparison.InvariantCultureIgnoreCase));
            }

            query = SortDescending
                ? OrderByDuration ? query.OrderByDescending(v => v.Duration) : (IEnumerable<Video>)query.OrderByDescending(v => v.Title)
                : OrderByDuration ? query.OrderBy(v => v.Duration) : (IEnumerable<Video>)query.OrderBy(v => v.Title);

            return query.ToList();
        }
    }

    public string Filter { get; set; } = string.Empty;

    public bool OrderByDuration { get; set; }

    public bool SortDescending { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await VideoService.FetchDataAsync();

        _videos.Clear();
        _videos.AddRange(VideoService.Videos);             
    }

    public void ToggleOrder() => OrderByDuration = !OrderByDuration;

    public void ToggleDescending() => SortDescending = !SortDescending;
}
