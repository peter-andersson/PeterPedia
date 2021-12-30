using PeterPedia.Shared;
using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;

namespace PeterPedia.Client.Pages.Videos;

public partial class Index : ComponentBase
{
    [Inject]
    public VideoService VideoService { get; set; } = null!;

    public List<Video> Videos { get; set; } = new List<Video>();

    protected override async Task OnInitializedAsync()
    {
        await VideoService.FetchData();

        Videos = VideoService.Videos;
    }
}