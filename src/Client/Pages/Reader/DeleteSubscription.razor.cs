using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Pages.Reader;

public partial class DeleteSubscription : ComponentBase
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

    public bool IsTaskRunning { get; set; } = false;
      
    public async Task Delete()
    {
        if (Subscription is null)
        {
            await OnClose.InvokeAsync();
            return;
        }

        IsTaskRunning = true;

        await RSSService.DeleteSubscriptionAsync(Subscription.Id);

        IsTaskRunning = false;

        await OnSuccess.InvokeAsync();
    }

    public async Task Cancel()
    {
        await OnClose.InvokeAsync();
    }    
}