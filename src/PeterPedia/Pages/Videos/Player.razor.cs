using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Pages.Videos;

public partial class Player : ComponentBase
{
    [Inject]
    private PeterPediaContext DbContext { get; set; } = null!;

    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    [Inject]
    private IFileService FileService { get; set; } = null!;

    [Inject]
    private ILogger<Player> Logger { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    [Parameter]
    public int Id { get; set; }

    public Video? Video { get; set; }

    protected override async Task OnInitializedAsync()
    {
        VideoEF? video = await DbContext.Videos.Where(v => v.Id == Id).SingleOrDefaultAsync();

        if (video is not null)
        {
            Video = new Video()
            {
                Id = video.Id,
                Title = video.Title,
                Duration = video.Duration,
                Type = video.Type,
            };

            var basePath = Configuration["VideoPath"] ?? "/video";
            var relativePath = Path.GetRelativePath(basePath, video.AbsolutePath);

            Video.Url = "/video/" + relativePath.Replace('\\', '/');
        }
    }

    public async Task DeleteAsync()
    {
        VideoEF? video = await DbContext.Videos.Where(v => v.Id == Id).AsTracking().SingleOrDefaultAsync();

        if (video is null)
        {
            LogMessage.VideoNotFound(Logger, Id);
            return;
        }

        FileService.Delete(video.AbsolutePath);

        DbContext.Remove(video);
        await DbContext.SaveChangesAsync();

        LogMessage.VideoDeleted(Logger, video.Title, video.AbsolutePath);

        Navigation.NavigateBack();
    }
}
