using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Reader;

public partial class Subscriptions : ComponentBase
{
    [Inject]
    private RSSService RSSService { get; set; } = null!;

    [CascadingParameter]
    private IModalService Modal { get; set; } = null!;

    private readonly List<Subscription> _subscriptions = new();

    public List<Subscription> SubscriptionList
    {
        get
        {
            IEnumerable<Subscription> query = string.IsNullOrWhiteSpace(Filter)
                ? _subscriptions
                : _subscriptions.Where(sub => sub.Title.Contains(Filter, StringComparison.InvariantCultureIgnoreCase));

            return query.OrderBy(sub => sub.Title).ToList();
        }
    }

    public string Filter { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await RSSService.FetchDataAsync();

        _subscriptions.Clear();
        _subscriptions.AddRange(RSSService.Subscriptions);
    }

    public void AddSubscription() => _ = Modal.Show<AddSubscriptionDialog>("Add subscription", new ModalOptions()
    {
        Class = "blazored-modal w-50",
    });
  
    public void EditSubscription(Subscription subscription)
    {
        var parameters = new ModalParameters();
        parameters.Add("Subscription", subscription);

        Modal.Show<EditSubscriptionDialog>("Edit subscription", parameters);
    }
}
