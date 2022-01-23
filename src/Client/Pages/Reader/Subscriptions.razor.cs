using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;
using PeterPedia.Shared;

namespace PeterPedia.Client.Pages.Reader;

public partial class Subscriptions : ComponentBase
{    
    [Inject]
    private RSSService RSSService { get; set; } = null!;
     
    public List<Subscription> SubscriptionList { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await RSSService.FetchData();

        FilterSubscription(string.Empty);
    }

    public void FilterSubscription(string filter)
    {
        IEnumerable<Subscription> subscriptions;

        if (string.IsNullOrWhiteSpace(filter))
        {
            subscriptions = RSSService.Subscriptions;
        }
        else
        {
            subscriptions = RSSService.Subscriptions.Where(sub => sub.Title.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
        }

        SubscriptionList = subscriptions.OrderBy(sub => sub.Title).ToList();
    } 
}