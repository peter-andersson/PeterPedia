using Microsoft.AspNetCore.Components;

namespace Reader.App.Pages;

public partial class ArticleView
{
    [Inject]
    private HttpClient Http { get; set; } = null!;

    [Parameter]
    public UnreadItem Article { get; set; } = null!;

    [Parameter]
    public EventCallback<UnreadItem> OnArticleRemove { get; set; }

    public bool IsTaskRunning { get; set; }

    private async Task DeleteAsync()
    {
        if (Article is null)
        {
            return;
        }

        IsTaskRunning = true;

        try
        {
            HttpResponseMessage response = await Http.DeleteAsync($"/api/deleteArticle/{Article.Id}");
            response.EnsureSuccessStatusCode();

            await OnArticleRemove.InvokeAsync(Article);
        }
        catch
        {
            // TODO: Handle error
        }
        finally
        {
            IsTaskRunning = false;
        }
    }
}
