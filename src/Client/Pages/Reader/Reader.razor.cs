using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Reader;

public partial class Reader : ComponentBase
{
    [Inject]
    RSSService RSSService { get; set; } = null!;

    private List<Subscription> SubscriptionList = null!;
    private Subscription? Subscription = null;

    protected override async Task OnInitializedAsync()
    {
        SubscriptionList = await RSSService.GetUnread();
    }

    private void Load(Subscription? subscription)
    {
        Subscription = subscription;
    }

    private void ArticleRemoved(Article article)
    {
        if (Subscription is null)
        {
            return;
        }

        Subscription.Articles.Remove(article);

        if (Subscription.Articles.Count == 0)
        {
            Subscription = null;

            StateHasChanged();
        }
    }
}