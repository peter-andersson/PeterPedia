using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Reader;

public partial class ArticleView
{
    [Inject]
    private RSSService RSSService { get; set; } = null!;

    [Parameter]
    public Article Article { get; set; } = null!;

    [Parameter]
    public EventCallback<Article> OnArticleRemove { get; set; }

    public bool IsTaskRunning { get; set; }

    private async Task DeleteAsync()
    {
        if (Article is null)
        {
            return;
        }

        IsTaskRunning = true;
        var result = await RSSService.DeleteArticleAsync(Article.Id);
        IsTaskRunning = false;

        if (result)
        {
            await OnArticleRemove.InvokeAsync(Article);
        }
    }
}
