using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PeterPedia.Client.Pages.Reader;

public partial class Subscriptions : ComponentBase
{
    [Inject]
    private RSSService RSSService { get; set; } = null!;

    public List<Subscription> SubscriptionList
    {
        get
        {
            IEnumerable<Subscription> subscriptions;

            if (string.IsNullOrWhiteSpace(Filter))
            {
                subscriptions = RSSService.Subscriptions;
            }
            else
            {
                subscriptions = RSSService.Subscriptions.Where(sub => sub.Title.Contains(Filter, StringComparison.InvariantCultureIgnoreCase));
            }

            return subscriptions.OrderBy(sub => sub.Title).ToList();
        }
    }

    public string AddSubscriptionElement { get; } = "add-subscription-dialog";

    public string DeleteSubscriptionElement { get; } = "delete-subscription-dialog";

    public string EditSubscriptionElement { get; } = "edit-subscription-dialog";

    public Subscription? SelectedSubscription { get; set; }

    public string Filter { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await RSSService.FetchDataAsync();
    }

    public async Task AddSubscription()
    {
        await ShowDialog(AddSubscriptionElement);
    }

    public async Task AddDialogClose()
    {
        await HideDialog(AddSubscriptionElement);
    }

    public async Task AddDialogSuccess()
    {
        await AddDialogClose();

        // FilterSubscription(_currentFilter);

        StateHasChanged();
    }

    public async Task DeleteSubscription(Subscription subscription)
    {
        SelectedSubscription = subscription;

        await ShowDialog(DeleteSubscriptionElement);
    }

    public async Task DeleteDialogClose()
    {
        SelectedSubscription = null;

        await HideDialog(DeleteSubscriptionElement);
    }

    public async Task DeleteDialogSuccess()
    {
        await DeleteDialogClose();

        // FilterSubscription(_currentFilter);

        StateHasChanged();
    }

    public async Task EditSubscription(Subscription subscription)
    {
        SelectedSubscription = subscription;

        await ShowDialog(EditSubscriptionElement);
    }

    public async Task EditDialogClose()
    {
        SelectedSubscription = null;

        await HideDialog(EditSubscriptionElement);
    }

    public async Task EditDialogSuccess()
    {
        await EditDialogClose();

        // FilterSubscription(_currentFilter);

        StateHasChanged();
    }

    private async Task ShowDialog(string element)
    {
        if (string.IsNullOrWhiteSpace(element))
        {
            return;
        }      
    }

    private async Task HideDialog(string element)
    {
        if (string.IsNullOrWhiteSpace(element))
        {
            return;
        }       
    }
}
