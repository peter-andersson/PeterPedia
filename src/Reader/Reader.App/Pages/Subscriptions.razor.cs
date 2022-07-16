using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace Reader.App.Pages;

public partial class Subscriptions : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;

    private Subscription[] SubscriptionList { get; set; } = Array.Empty<Subscription>();

    private bool Loading { get; set; } = true;
    
    protected override async Task OnInitializedAsync()
    {
        Loading = true;

        SubscriptionList = await Http.GetFromJsonAsync<Subscription[]>("/api/all") ?? Array.Empty<Subscription>();

        Loading = false;
    }
}
