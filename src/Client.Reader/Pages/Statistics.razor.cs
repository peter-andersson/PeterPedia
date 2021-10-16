using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Reader.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeterPedia.Client.Reader.Pages
{
    public partial class Statistics : ComponentBase
    {
        [Inject]
        RSSService RSSService { get; set; }

        private int ArticleCount { get; set; }
        private int SubscriptionCount { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await RSSService.FetchData();

            ArticleCount = await RSSService.GetArticleCount();
            SubscriptionCount = RSSService.Subscriptions.Count;
        }
    }
}
