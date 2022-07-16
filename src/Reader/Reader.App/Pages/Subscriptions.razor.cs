using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Reader;

public partial class Subscriptions : ComponentBase
{
    [Inject]
    private IReaderManager ReaderManager { get; set; } = null!;

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
        List<Subscription> subscriptions = await ReaderManager.GetSubscriptionsAsync();

        _subscriptions.Clear();
        _subscriptions.AddRange(subscriptions);
    }
}
