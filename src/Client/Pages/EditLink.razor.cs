using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Pages;

public partial class EditLink : ComponentBase
{
    [Inject]
    private LinkService LinkService { get; set; } = null!;

    [Parameter]
    public string? Id { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnClose { get; set; }

    [Parameter, AllowNull]
    public List<Link> Links { get; set; }

    public Link Link { get; set; } = new Link();

    public bool IsTaskRunning { get; set; } = false;

    public void Edit(Link link)
    {
        Link = link;
    }

    public async Task Save()
    {
        IsTaskRunning = true;

        await LinkService.Upsert(Link);

        Link = new Link();
        IsTaskRunning = false;
    }

    public async Task Delete()
    {
        if (Link.Id == 0)
        {
            return;
        }

        IsTaskRunning = true;

        await LinkService.Delete(Link.Id);

        Links.Remove(Link);

        Link = new Link();

        IsTaskRunning = false;
    }

    public async Task Close()
    {
        await OnClose.InvokeAsync();
    }
}