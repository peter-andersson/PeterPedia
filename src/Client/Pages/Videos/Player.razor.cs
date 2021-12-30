using PeterPedia.Shared;
using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;

namespace PeterPedia.Client.Pages.Videos;

public partial class Player : ComponentBase
{
    [Inject]
    public VideoService VideoService { get; set; } = null!;

    [Inject]
    private NavigationManager NavManager { get; set; } = null!;

    [Parameter]
    public int Id { get; set; }

    public Video? Video { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Video = await VideoService.GetVideoAsync(Id);        
    }

    public async Task Delete()
    {
        if (await VideoService.Delete(Id))
        {
            NavManager.NavigateTo("videos");
        }
    }
}