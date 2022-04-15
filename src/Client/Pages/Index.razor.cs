using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages;

public partial class Index : ComponentBase
{
    [Inject]
    private SyncService SyncService { get; set; } = null!;

    [Inject]
    public LinkService LinkService { get; set; } = null!;

    public List<Link> Links { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        SyncService.Start();

        Links = await LinkService.GetLinksAsync();

        Links.Add(new Link()
        {
            Title = "Books",
            Url = "books"
        });

        Links.Add(new Link()
        {
            Title = "Episodes",
            Url = "episodes"
        });

        Links.Add(new Link()
        {
            Title = "Movies",
            Url = "movies"
        });

        Links.Add(new Link()
        {
            Title = "Photos",
            Url = "photos"
        });

        Links.Add(new Link()
        {
            Title = "Reader",
            Url = "reader"
        });

        Links.Add(new Link()
        {
            Title = "Videos",
            Url = "videos"
        });

        Links.Sort((link1, link2) => link1.Title.CompareTo(link2.Title));
    }
}
