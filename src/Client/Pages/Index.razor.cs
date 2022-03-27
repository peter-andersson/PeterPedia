using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PeterPedia.Client.Pages;

public partial class Index : ComponentBase
{
    private IJSObjectReference _module = null!;

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    [Inject]
    private SyncService SyncService { get; set; } = null!;

    [Inject]
    public LinkService LinkService { get; set; } = null!;

    [Inject]
    public IToastService ToastService { get; set; } = null!;

    public List<Link> Links { get; set; } = null!;

    public string LinksElement { get; set; } = "edit-links-dialog";

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/dialog.js");
    }

    public async Task EditAsync() => await ShowDialogAsync(LinksElement);

    public async Task DialogCloseAsync() => await HideDialogAsync(LinksElement);

    private async Task ShowDialogAsync(string element)
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

    private async Task HideDialogAsync(string element)
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
