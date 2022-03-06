using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Videos;

public partial class Videos : ComponentBase
{
    [Inject]
    public VideoService VideoService { get; set; } = null!;

    public List<Video> VideoList { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await VideoService.FetchDataAsync();

        FilterVideos(string.Empty);
    }

    public void FilterVideos(string filter)
    {
        VideoList = VideoService.Videos.Where(v => v.Title.Contains(filter, StringComparison.InvariantCultureIgnoreCase)).ToList();
    }
}