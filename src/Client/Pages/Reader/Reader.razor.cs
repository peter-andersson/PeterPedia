using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Reader;

public partial class Reader : ComponentBase
{
    [Inject]
    private RSSService RSSService { get; set; } = null!;

    public List<UnreadArticle> UnreadArticles = new();
    public UnreadArticle? Unread = null;

    protected override async Task OnInitializedAsync()
    {
        UnreadArticles = await RSSService.GetUnreadAsync();

        foreach (UnreadArticle unread in UnreadArticles)
        {
            unread.Articles = unread.Articles.OrderBy(a => a.PublishDate).ToList();
        }
    }

    private void Load(UnreadArticle? unread) => Unread = unread;

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
