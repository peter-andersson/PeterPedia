using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;
using PeterPedia.Shared;

namespace PeterPedia.Client.Pages.Reader;

public partial class Edit : ComponentBase
{
    [Inject]
    private RSSService RSSService { get; set; } = null!;

    [Inject]
    private NavigationManager NavManager { get; set; } = null!;

    [Parameter]
    public int Id { get; set; }

    private bool IsTaskRunning;

    private Subscription? Subscription;

    protected override async Task OnInitializedAsync()
    {
        IsTaskRunning = false;
        var subscription = await RSSService.GetSubscription(Id);

        if (subscription is null)
        {
            NavManager.NavigateTo("subscriptions");
        }
        else
        {
            Subscription = CreateCopy(subscription);
        }
    }

    private async Task Save()
    {
        if (Subscription is null)
        {
            return;
        }

        IsTaskRunning = true;

        var result = await RSSService.UpdateSubscription(Subscription);

        IsTaskRunning = false;
        if (result)
        {
            NavManager.NavigateTo("subscriptions");
        }
    }

    private void Cancel()
    {
        NavManager.NavigateTo("subscriptions");
    }

    private static Subscription CreateCopy(Subscription subscription)
    {
        var result = new Subscription()
        {
            Id = subscription.Id,
            Title = subscription.Title,
            UpdateIntervalMinute = subscription.UpdateIntervalMinute,
            LastUpdate = subscription.LastUpdate,
            Url = subscription.Url,
        };

        return result;
    }
}