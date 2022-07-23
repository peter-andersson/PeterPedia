using Microsoft.AspNetCore.Components;

namespace Reader.App.Pages;

public partial class View : ComponentBase
{
    [Inject]
    private IReaderService Service { get; set; } = null!;

    [Parameter]
    public string Group { get; set; } = string.Empty;

    private UnreadGroup? Unread { get; set; } = null;

    protected override void OnInitialized() => Unread = Service.GetUnreadGroupAsync(Group);

    private void ArticleRemoved(UnreadItem article)
    {
        if (Unread is null)
        {
            return;
        }

        Unread.Items.Remove(article);

        StateHasChanged();
    }
}
