using Microsoft.AspNetCore.Components;

namespace Reader.App.Pages;

public partial class Index : ComponentBase
{
    [Inject]
    private IReaderService Service { get; set; } = null!;

    private UnreadGroup[] UnreadArticles { get; set; } = Array.Empty<UnreadGroup>();

    private bool Loading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        UnreadArticles = await Service.UnreadArticlesAsync();

        Loading = false;
    }  
}
