using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Pages.Videos;

public partial class Videos : ComponentBase
{
    [Inject]
    private PeterPediaContext DbContext { get; set; } = null!;

    public readonly List<Video> _videos = new();

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
        List<VideoEF> items = await DbContext.Videos.ToListAsync();

        _videos.Clear();

        foreach (VideoEF item in items)
        {
            _videos.Add(new Video()
            {
                Id = item.Id,
                Title = item.Title,
                Duration = item.Duration,
                Type = item.Type,
            });
        }           
    }

    public void ToggleOrder() => OrderByDuration = !OrderByDuration;

    public void ToggleDescending() => SortDescending = !SortDescending;
}
