using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace Reader.App.Pages;

public partial class Index : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;

    private UnreadGroup[] UnreadArticles { get; set; } = Array.Empty<UnreadGroup>();

    private UnreadGroup? Unread { get; set; } = null;

    private bool Loading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        throw new NotImplementedException();

        UnreadArticles = await Http.GetFromJsonAsync<UnreadGroup[]>("/api/unread") ?? Array.Empty<UnreadGroup>();

        Loading = false;
    }

    private void Load(UnreadGroup? unread) => Unread = unread;

    private void ArticleRemoved(UnreadItem article)
    {
        if (Unread is null)
        {
            return;
        }

        Unread.Items.Remove(article);

        if (Unread.Items.Count == 0)
        {
            Unread = null;

            StateHasChanged();
        }
    }
}
