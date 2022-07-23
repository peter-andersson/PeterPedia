using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Reader.App.Pages;

public partial class Add : ComponentBase
{
    [Inject]
    private IReaderService Service { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    private bool IsTaskRunning { get; set; } = false;

    private NewSubscription NewSubscription { get; set; } = new();

    private List<string> Urls { get; set; } = new List<string>();

    private InputText? Input { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (Input?.Element != null)
            {
                await Input.Element.Value.FocusAsync();
            }
        }
    }

    public async Task AddAsync()
    {
        if (string.IsNullOrEmpty(NewSubscription.Url))
        {
            return;
        }

        Urls.Clear();
        IsTaskRunning = true;

        AddResult result = await Service.AddAsync(NewSubscription);

        if (string.IsNullOrWhiteSpace(result.ErrorMessage) && result.Urls.Count == 0)
        {
            ToastService.ShowSuccess("Subscription added");
            NewSubscription.Url = string.Empty;
        }
        else if (result.Urls.Count > 0)
        {
            Urls.Clear();

            Urls.AddRange(result.Urls);
        }
        else
        {
            ToastService.ShowError(result.ErrorMessage);
        }

        IsTaskRunning = false;
    }
}
