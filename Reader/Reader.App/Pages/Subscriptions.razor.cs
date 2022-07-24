using Microsoft.AspNetCore.Components;

namespace Reader.App.Pages;

public partial class Subscriptions : ComponentBase
{
    [Inject]
    private IReaderService Service { get; set; } = null!;

    private Subscription[] SubscriptionList { get; set; } = Array.Empty<Subscription>();

    private bool Loading { get; set; } = true;
    
    protected override async Task OnInitializedAsync()
    {
        Loading = true;

        SubscriptionList = await Service.GetSubscriptionsAsync();

        Loading = false;
    }
}
