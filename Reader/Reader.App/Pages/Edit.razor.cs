using Microsoft.AspNetCore.Components;

namespace Reader.App.Pages;

public partial class Edit : ComponentBase
{
    [Inject]
    private IReaderService Service { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    [Parameter]
    public string Id { get; set; } = string.Empty;

    private EditModel EditModel { get; set; } = new();

    private Subscription? Subscription { get; set; }

    private bool IsSaveTaskRunning { get; set; }

    private bool IsDeleteTaskRunning { get; set; }

    private bool Loading { get; set; } = true;

    protected override void OnInitialized()
    {
        IsSaveTaskRunning = false;
        IsDeleteTaskRunning = false;
        Loading = true;

        Subscription = Service.GetSubscription(Id);

        if (Subscription is not null)
        {
            EditModel.Title = Subscription.Title;
            EditModel.Group = Subscription.Group;
            EditModel.UpdateIntervalMinute = Subscription.UpdateIntervalMinute;
            EditModel.UpdateAt = Subscription.UpdateAt;
            EditModel.Url = Subscription.Url;
        }

        Loading = false;
    }

    public async Task SaveAsync()
    {
        if (Subscription is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(EditModel.Title))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(EditModel.Url))
        {
            return;
        }

        Subscription.Title = EditModel.Title;
        Subscription.Group = EditModel.Group;
        Subscription.UpdateIntervalMinute = EditModel.UpdateIntervalMinute;
        Subscription.UpdateAt = EditModel.UpdateAt;
        Subscription.Url = EditModel.Url;

        IsSaveTaskRunning = true;

        if (await Service.UpdateSubscriptionAsync(Subscription))
        {
            ToastService.ShowSuccess("Subscription saved");
        }
        else
        {
            ToastService.ShowError("Failed to save subscription");
        }
        
        IsSaveTaskRunning = false;
    }

    private async Task DeleteAsync()
    {
        if (Subscription is null)
        {
            return;
        }

        IsDeleteTaskRunning = true;

        if (await Service.UpdateSubscriptionAsync(Subscription))
        {
            NavigateBack();
        }
        else
        {
            ToastService.ShowError("Failed to delete subscription");
        }

        IsDeleteTaskRunning = false;
    }

    private void Close() => NavigateBack();


    private void NavigateBack()
    {
        if (Navigation.CanNavigateBack)
        {
            Navigation.NavigateBack();
        }
        else
        {
            Navigation.NavigateTo("/subscriptions");
        }
    }
}
