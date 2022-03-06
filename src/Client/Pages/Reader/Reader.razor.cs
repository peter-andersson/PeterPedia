using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Reader;

public partial class Reader : ComponentBase
{
    [Inject]
    RSSService RSSService { get; set; } = null!;

    private List<UnreadArticle> UnreadArticles = null!;
    private UnreadArticle? Unread = null;

    protected override async Task OnInitializedAsync()
    {
        UnreadArticles = await RSSService.GetUnreadAsync();
    }

    private void Load(UnreadArticle? unread)
    {
        Unread = unread;
    }

    private void ArticleRemoved(Article article)
    {
        if (Unread is null)
        {
            return;
        }

        Unread.Articles.Remove(article);

        if (Unread.Articles.Count == 0)
        {
            Unread = null;

            StateHasChanged();
        }
    }
}