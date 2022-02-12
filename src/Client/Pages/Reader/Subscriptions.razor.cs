using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PeterPedia.Client.Services;
using PeterPedia.Shared;

namespace PeterPedia.Client.Pages.Reader;

public partial class Subscriptions : ComponentBase
{
    private IJSObjectReference _module = null!;

    [Inject]
    private RSSService RSSService { get; set; } = null!;

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    public List<Subscription> SubscriptionList { get; set; } = null!;

    public string AddSubscriptionElement { get; } = "add-subscription-dialog";

    public string DeleteSubscriptionElement { get; } = "delete-subscription-dialog";

    public string EditSubscriptionElement { get; } = "edit-subscription-dialog";

    public Subscription? SelectedSubscription { get; set; }

    private string _currentFilter = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await RSSService.FetchData();

        FilterSubscription(string.Empty);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/dialog.js");
    }

    public void FilterSubscription(string filter)
    {
        _currentFilter = filter;

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

        FilterSubscription(_currentFilter);

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

        FilterSubscription(_currentFilter);

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

        FilterSubscription(_currentFilter);

        StateHasChanged();
    }

    private async Task ShowDialog(string element)
    {
        if (string.IsNullOrWhiteSpace(element))
        {
            return;
        }

        if (_module is not null)
        {
            await _module.InvokeVoidAsync("ShowDialog", element);
        }
    }

    private async Task HideDialog(string element)
    {
        if (string.IsNullOrWhiteSpace(element))
        {
            return;
        }

        if (_module is not null)
        {
            await _module.InvokeVoidAsync("HideDialog", element);
        }
    }
}