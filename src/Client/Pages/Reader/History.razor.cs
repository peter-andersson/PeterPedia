using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;
using PeterPedia.Shared;

namespace PeterPedia.Client.Pages.Reader;

public partial class History : ComponentBase
{
    [Inject]
    private RSSService RSSService { get; set; } = null!;

    public List<Article> Articles { get; set; } = null!;

    private List<Article> _articles = null!;

    protected override async Task OnInitializedAsync()
    {
        _articles = await RSSService.GetHistoryAsync();

        FilterArticles(string.Empty);
    }

    public void FilterArticles(string filter)
    {
        IEnumerable<Article> articles;

        if (string.IsNullOrWhiteSpace(filter))
        {
            articles = _articles;
        }
        else
        {
            articles = _articles.Where(a => a.Title.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
        }

        Articles = articles.ToList();
    }
}