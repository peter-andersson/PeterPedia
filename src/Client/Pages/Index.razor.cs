using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PeterPedia.Client.Pages;

public partial class Index : ComponentBase
{
    private IJSObjectReference _module = null!;

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    [Inject]
    public LinkService LinkService { get; set; } = null!;

    [Inject]
    public IToastService ToastService { get; set; }

    public List<Link> Links { get; set; } = null!;

    public string LinksElement { get; set; } = "edit-links-dialog";

    protected override async Task OnInitializedAsync()
    {
        Links = await LinkService.GetLinks();

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/dialog.js");
    }

    public async Task Edit()
    {
        await ShowDialog(LinksElement);
    }

    public async Task DialogClose()
    {
        await HideDialog(LinksElement);
    }

    private async Task ShowDialog(string element)
    {
        if (string.IsNullOrWhiteSpace(element))
        {
            return;
        }

        if (_module is not null)
        {
            await _module.InvokeVoidAsync("ShowDialog", element);
        }
    }

    private async Task HideDialog(string element)
    {
        if (string.IsNullOrWhiteSpace(element))
        {
            return;
        }

        if (_module is not null)
        {
            await _module.InvokeVoidAsync("HideDialog", element);
        }
    }
}