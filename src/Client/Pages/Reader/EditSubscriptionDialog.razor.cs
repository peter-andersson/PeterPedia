using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Reader;

public partial class EditSubscriptionDialog : ComponentBase
{
    [Inject]
    private RSSService RSSService { get; set; } = null!;

    [CascadingParameter]
    private BlazoredModalInstance ModalInstance { get; set; } = null!;

    [Parameter]
    public Subscription Subscription { get; set; } = null!;

    public SubscriptionModel EditSubscription { get; set; } = new SubscriptionModel();

    public bool IsTaskRunning { get; set; }

    protected override void OnInitialized()
    {
        IsTaskRunning = false;

        EditSubscription.Title = Subscription.Title;
        EditSubscription.Group = Subscription.Group;
        EditSubscription.UpdateIntervalMinute = Subscription.UpdateIntervalMinute;
    }

    public async Task SaveAsync()
    {
        IsTaskRunning = true;

        if (!string.IsNullOrWhiteSpace(EditSubscription.Title))
        {
            Subscription.Title = EditSubscription.Title;
        }

        Subscription.UpdateIntervalMinute = EditSubscription.UpdateIntervalMinute;
        Subscription.Group = EditSubscription.Group;

        var result = await RSSService.UpdateSubscriptionAsync(Subscription);

        IsTaskRunning = false;
        if (result)
        {
            await ModalInstance.CancelAsync();
        }
    }

    private async Task DeleteAsync()
    {
        IsTaskRunning = true;

        if (await RSSService.DeleteSubscriptionAsync(Subscription.Id))
        {
            await ModalInstance.CloseAsync();
        }

        IsTaskRunning = false;
    }

    private async Task CloseAsync() => await ModalInstance.CancelAsync();
}
