using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace Reader.App.Pages;

public partial class History : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;

    private HistoryArticle[] Articles { get; set; } = Array.Empty<HistoryArticle>();

    private bool Loading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        Loading = true;

        Articles = await Http.GetFromJsonAsync<HistoryArticle[]>("/api/history") ?? Array.Empty<HistoryArticle>();

        Loading = false;
    }
}
