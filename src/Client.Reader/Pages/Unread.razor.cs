using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Reader.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using PeterPedia.Shared;

namespace PeterPedia.Client.Reader.Pages
{
    public partial class Unread : ComponentBase
    {
        [Inject]
        RSSService RSSService { get; set; }

        private List<Subscription> SubscriptionList;
        private Subscription Subscription;

        protected override async Task OnInitializedAsync()
        {
            SubscriptionList = await RSSService.GetUnread();
        }

        private void Load(Subscription subscription)
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
}
