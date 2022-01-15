using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PeterPedia.Client.Services;

namespace PeterPedia.Client.Pages.Reader;

public partial class Add : ComponentBase
{
    [Inject]
    private RSSService RSSService { get; set; } = null!;

    private EditContext AddContext = null!;
    private string NewSubscriptionUrl = null!;
    private bool IsTaskRunning;

    protected override void OnInitialized()
    {
        IsTaskRunning = false;
        NewSubscriptionUrl = string.Empty;
        AddContext = new EditContext(NewSubscriptionUrl);
    }

    private async Task AddSubscription()
    {
        IsTaskRunning = true;

        await RSSService.AddSubscription(NewSubscriptionUrl);
        IsTaskRunning = false;
    }
}