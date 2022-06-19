using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace PeterPedia.Pages;

public partial class Index : ComponentBase
{    
    [Inject]
    private PeterPediaContext DbContext { get; set; } = null!;

    [Inject]
    private IMemoryCache Cache { get; set; } = null!;

    public List<Link> Links { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Links.Clear();

        if (Cache.TryGetValue(CacheKey.Links, out List<Link> list))
        {
            Links.AddRange(list);
            return;
        }

        List<LinkEF> links = await DbContext.Links.ToListAsync();

        Links.Clear();

        foreach (LinkEF link in links)
        {
            Links.Add(new Link()
            {
                Id = link.Id,
                Title = link.Title,
                Url = link.Url,
            });
        }
       
        Links.Add(new Link()
        {
            Title = "Library",
            Url = "library/books"
        });

        Links.Add(new Link()
        {
            Title = "TV Shows",
            Url = "tv"
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

        Cache.Set(CacheKey.Links, Links, TimeSpan.FromMinutes(5));
    }
}
