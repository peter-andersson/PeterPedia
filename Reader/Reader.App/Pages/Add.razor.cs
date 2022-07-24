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

    private AddModel AddModel { get; set; } = new();

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
        if (string.IsNullOrWhiteSpace(AddModel.Url))
        {
            return;
        }

        Urls.Clear();
        IsTaskRunning = true;

        var newSubscription = new NewSubscription()
        {
            Url = AddModel.Url
        };

        AddResult result = await Service.AddAsync(newSubscription);

        if (string.IsNullOrWhiteSpace(result.ErrorMessage) && result.Urls.Count == 0)
        {
            ToastService.ShowSuccess("Subscription added");
            AddModel.Url = string.Empty;
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
