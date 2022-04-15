using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages;

public partial class EditLink : ComponentBase
{
    [Inject]
    private LinkService LinkService { get; set; } = null!;

    public Link Link { get; set; } = new Link();

    public bool IsTaskRunning { get; set; } = false;

    public void Edit(Link link) => Link = link;

    public List<Link> Links { get; set; } = null!;

    protected override async Task OnInitializedAsync() => Links = await LinkService.GetLinksAsync();

    public async Task SaveAsync()
    {
        IsTaskRunning = true;

        await LinkService.UpsertAsync(Link);
        
        Link = new Link();
        IsTaskRunning = false;
    }

    public async Task DeleteAsync()
    {
        if (Link.Id == 0)
        {
            return;
        }

        IsTaskRunning = true;

        await LinkService.DeleteAsync(Link.Id);

        Links.Remove(Link);

        Link = new Link();

        IsTaskRunning = false;
    }
}
