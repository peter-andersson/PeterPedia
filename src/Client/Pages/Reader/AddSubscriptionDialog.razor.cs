using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PeterPedia.Client.Pages.Reader;

public partial class AddSubscriptionDialog : ComponentBase
{
    [Inject]
    private RSSService RSSService { get; set; } = null!;

    [CascadingParameter]
    private BlazoredModalInstance ModalInstance { get; set; } = null!;
   
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

        var result = await RSSService.AddSubscriptionAsync(NewSubscriptionUrl);

        IsTaskRunning = false;

        if (result)
        {
            NewSubscriptionUrl = string.Empty;

            await ModalInstance.CloseAsync();
        }
    }

    private async Task CloseAsync() => await ModalInstance.CancelAsync();  
}
