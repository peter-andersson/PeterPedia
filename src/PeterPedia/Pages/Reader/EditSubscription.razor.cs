using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Reader;

public partial class EditSubscription : ComponentBase
{
    [Inject]
    private IReaderManager ReaderManager { get; set; } = null!;

    [Inject]
    private NavigationManager NavManager { get; set; } = null!;

    [Parameter]
    public int Id { get; set; }

    public Subscription Subscription { get; set; } = new Subscription();

    public bool IsTaskRunning { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsTaskRunning = false;

        Subscription? sub = await ReaderManager.GetSubscriptionAsync(Id);
        if (sub is not null)
        {
            Subscription.Id = Id;
            Subscription.Title = sub.Title;
            Subscription.Group = sub.Group;
            Subscription.UpdateIntervalMinute = sub.UpdateIntervalMinute;
            Subscription.Url = sub.Url;
        } 
    }

    public async Task SaveAsync()
    {
        IsTaskRunning = true;

        var result = await ReaderManager.UpdateSubscriptionAsync(Subscription);

        IsTaskRunning = false;
        if (result)
        {
            NavManager.NavigateTo("/reader/subscriptions");
        }
    }

    private async Task DeleteAsync()
    {
        IsTaskRunning = true;

        if (await ReaderManager.DeleteSubscriptionAsync(Id))
        {
            NavManager.NavigateTo("/reader/subscriptions");
        }

        IsTaskRunning = false;
    }

    private void Close() => NavManager.NavigateTo("/reader/subscriptions");
}
