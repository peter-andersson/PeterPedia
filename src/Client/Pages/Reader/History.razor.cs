using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Reader;

public partial class History : ComponentBase
{
    [Inject]
    private RSSService RSSService { get; set; } = null!;

    public List<Article> Articles { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        List<Article> articles = await RSSService.GetHistoryAsync();

        Articles.AddRange(articles);
    }   
}
