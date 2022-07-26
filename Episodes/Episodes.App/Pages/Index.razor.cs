using Microsoft.AspNetCore.Components;

namespace Episodes.App.Pages;

public partial class Index : ComponentBase
{
    [Inject]
    private ITVService Service { get; set; } = null!;

    private TVShow[] ShowList { get; set; } = Array.Empty<TVShow>();

    private bool Loading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        ShowList = await Service.GetWatchListAsync();

        Loading = false;
    }
}
