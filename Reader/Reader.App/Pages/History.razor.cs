using Microsoft.AspNetCore.Components;

namespace Reader.App.Pages;

public partial class History : ComponentBase
{
    [Inject]
    private IReaderService Service { get; set; } = null!;

    private HistoryArticle[] Articles { get; set; } = Array.Empty<HistoryArticle>();

    private bool Loading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        Loading = true;

        Articles = await Service.GetHistoryAsync();

        Loading = false;
    }
}
