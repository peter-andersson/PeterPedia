using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Reader;

public partial class ArticleView
{
    [Inject]
    private IReaderManager ReaderManager { get; set; } = null!;

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
        var result = await ReaderManager.DeleteArticleAsync(Article.Id);
        IsTaskRunning = false;

        if (result)
        {
            await OnArticleRemove.InvokeAsync(Article);
        }
    }
}
