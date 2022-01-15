using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;
using PeterPedia.Shared;

namespace PeterPedia.Client.Pages.Reader;

public partial class History : ComponentBase
{
    [Inject]
    private RSSService RSSService { get; set; } = null!;

    private List<Article> Articles = null!;

    protected override async Task OnInitializedAsync()
    {
        Articles = await RSSService.GetHistory();
    }
}