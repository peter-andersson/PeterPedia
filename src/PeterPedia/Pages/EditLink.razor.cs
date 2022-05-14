using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace PeterPedia.Pages;

public partial class EditLink : ComponentBase
{
    [Inject]
    private PeterPediaContext DbContext { get; set; } = null!;

    [Inject]
    private IMemoryCache Cache { get; set; } = null!;

    public Link Link { get; set; } = new Link();

    public bool IsTaskRunning { get; set; } = false;

    public List<Link> Links { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
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
    }

    public void Edit(Link link) => Link = link;

    public async Task SaveAsync()
    {
        IsTaskRunning = true;

        LinkEF? linkEF = await DbContext.Links.FindAsync(Link.Id);

        if (linkEF is null)
        {
            linkEF = new LinkEF();
        }

        linkEF.Title = Link.Title;
        linkEF.Url = Link.Url;

        if (linkEF.Id > 0)
        {
            DbContext.Links.Update(linkEF);
        }
        else
        {
            DbContext.Links.Add(linkEF);
            Links.Add(Link);
        }

        await DbContext.SaveChangesAsync();

        Link = new Link();
        IsTaskRunning = false;

        Cache.Remove(CacheKey.Links);
    }

    public async Task DeleteAsync()
    {
        if (Link.Id == 0)
        {
            return;
        }

        IsTaskRunning = true;

        LinkEF? linkEF = await DbContext.Links.Where(l => l.Id == Link.Id).AsTracking().SingleOrDefaultAsync();

        if (linkEF is null)
        {
            return;
        }

        DbContext.Links.Remove(linkEF);
        await DbContext.SaveChangesAsync();

        Links.Remove(Link);

        Link = new Link();

        IsTaskRunning = false;

        Cache.Remove(CacheKey.Links);
    }
}
