using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Pages.Photos;

public partial class Photos : ComponentBase
{
    [Inject]
    private PeterPediaContext DbContext { get; set; } = null!;

    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    public List<string> Albums { get; set; } = new();

    public string? Album { get; set; }

    public List<Photo>? PhotoList { get; set; } = null;

    protected override async Task OnInitializedAsync()
    {
        List<AlbumEF> items = await DbContext.Albums.OrderBy(a => a.Name).ToListAsync();

        Albums.Clear();
        foreach (AlbumEF item in items)
        {
            Albums.Add(item.Name);
        }       
    }

    public async Task OpenAlbumAsync(string album)
    {
        if (string.IsNullOrWhiteSpace(album))
        {
            PhotoList = null;
            return;
        }

        List<PhotoEF> items = await DbContext.Photos.Where(p => p.Album.Name == album).ToListAsync();

        PhotoList = new List<Photo>(items.Count);
        foreach (PhotoEF item in items)
        {
            PhotoList.Add(ConvertToItem(item));
        }
    }

    private Photo ConvertToItem(PhotoEF itemEF)
    {
        if (itemEF is null)
        {
            throw new ArgumentNullException(nameof(itemEF));
        }

        var photo = new Photo();

        var basePath = Configuration["PhotoPath"] ?? "/photos";
        var relativePath = Path.GetRelativePath(basePath, itemEF.AbsolutePath);

        photo.Url = "/photo/" + relativePath.Replace('\\', '/');

        return photo;
    }
}
