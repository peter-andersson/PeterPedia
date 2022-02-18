using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;
using PeterPedia.Shared;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Pages.Reader;

public partial class EditSubscription : ComponentBase
{
    [Inject]
    private RSSService RSSService { get; set; } = null!;

    [Parameter]
    public string? Id { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnClose { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnSuccess { get; set; }

    [Parameter, AllowNull]
    public Subscription? Subscription { get; set; }

    public bool IsTaskRunning { get; set; }

    protected override void OnInitialized()
    {
        IsTaskRunning = false;

        if (Subscription is not null)
        {
            Subscription = CreateCopy(Subscription);
        }
    }

    public async Task Save()
    {
        if (Subscription is null)
        {
            await OnClose.InvokeAsync();
            return;
        }

        IsTaskRunning = true;

        var result = await RSSService.UpdateSubscription(Subscription);

        IsTaskRunning = false;
        if (result)
        {
            await OnSuccess.InvokeAsync();
        }
    }

    public async Task Cancel()
    {
        await OnClose.InvokeAsync();
    }    

    private static Subscription CreateCopy(Subscription subscription)
    {
        var result = new Subscription()
        {
            Id = subscription.Id,
            Title = subscription.Title,
            Group = subscription.Group,
            UpdateIntervalMinute = subscription.UpdateIntervalMinute,
            LastUpdate = subscription.LastUpdate,
            Url = subscription.Url,
        };

        return result;
    }
}