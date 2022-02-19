using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Photos;

public partial class Photos : ComponentBase
{
    [Inject]
    public PhotoService PhotoService { get; set; } = null!;

    public List<string> Albums { get; set; } = null!;

    public string? Album { get; set; }

    public List<Photo>? PhotoList { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await PhotoService.FetchData();

        Albums = new List<string>();
        foreach (Photo photo in PhotoService.Photos)
        {
            if (!Albums.Contains(photo.Album))
            {
                Albums.Add(photo.Album);
            }
        }
    }

    public void OpenAlbum(string album)
    {
        if (string.IsNullOrWhiteSpace(album))
        {
            PhotoList = null;
            return;
        }

        Album = album;
        PhotoList = PhotoService.Photos.Where(p => p.Album == album).ToList();
    }
}