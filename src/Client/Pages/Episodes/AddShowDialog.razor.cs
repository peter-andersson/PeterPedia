using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PeterPedia.Client.Pages.Episodes;

public partial class AddShowDialog: ComponentBase
{
    [Inject]
    private IEpisodeManager EpisodeManager { get; set; } = null!;

    [CascadingParameter]
    private BlazoredModalInstance ModalInstance { get; set; } = null!;

    public bool IsTaskRunning { get; set; } = false;

    public string ShowUrl { get; set; } = string.Empty;

    public async Task InputKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await AddAsync();
        }
    }

    public async Task AddAsync()
    {
        if (string.IsNullOrEmpty(ShowUrl))
        {
            return;
        }

        IsTaskRunning = true;

        var result = await EpisodeManager.AddAsync(ShowUrl);

        IsTaskRunning = false;

        if (result)
        {
            ShowUrl = string.Empty;

            await ModalInstance.CloseAsync();
        }
    }

    private async Task CloseAsync() => await ModalInstance.CancelAsync();
}
