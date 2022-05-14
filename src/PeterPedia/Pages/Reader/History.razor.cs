using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Reader;

public partial class History : ComponentBase
{
    [Inject]
    private IReaderManager ReaderManager { get; set; } = null!;

    public List<Article> Articles { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        List<Article> articles = await ReaderManager.GetHistoryAsync();

        Articles.AddRange(articles);
    }   
}
