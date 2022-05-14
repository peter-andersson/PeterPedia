using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PeterPedia.Pages.Reader;

public partial class AddSubscription : ComponentBase
{
    [Inject]
    private IReaderManager ReaderManager { get; set; } = null!;

    [Inject]
    private NavigationManager NavManager { get; set; } = null!;

    public bool IsTaskRunning { get; set; } = false;

    public string NewSubscriptionUrl { get; set; } = string.Empty;

    public async Task InputKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await AddAsync();
        }
    }

    public async Task AddAsync()
    {
        if (string.IsNullOrEmpty(NewSubscriptionUrl))
        {
            return;
        }

        IsTaskRunning = true;

        var result = await ReaderManager.AddSubscriptionAsync(NewSubscriptionUrl);

        IsTaskRunning = false;

        if (result)
        {
            NewSubscriptionUrl = string.Empty;

            NavManager.NavigateTo("/reader/subscriptions");
        }
    }

    private void Close() => NavManager.NavigateTo("/reader/subscriptions");
}
