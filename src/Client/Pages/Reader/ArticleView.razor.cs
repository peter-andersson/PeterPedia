using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;
using PeterPedia.Shared;

namespace PeterPedia.Client.Pages.Reader;

public partial class ArticleView
{
    [Inject]
    RSSService RSSService { get; set; } = null!;

    [Parameter]
    public Article Article { get; set; } = null!;

    [Parameter]
    public EventCallback<Article> OnArticleRemove { get; set; }

    private bool IsTaskRunning;

    private async Task Delete()
    {
        if (Article is null)
        {
            return;
        }

        IsTaskRunning = true;
        var result = await RSSService.DeleteArticle(Article.Id);
        IsTaskRunning = false;

        if (result)
        {
            await OnArticleRemove.InvokeAsync(Article);
        }
    }
}