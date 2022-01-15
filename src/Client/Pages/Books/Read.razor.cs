using Microsoft.AspNetCore.Components;
using PeterPedia.Shared;
using Microsoft.AspNetCore.Components.Forms;
using PeterPedia.Client.Services;

namespace PeterPedia.Client.Pages.Books;

public partial class Read : ComponentBase
{
    [Inject]
    BookService BookService { get; set; } = null!;

    private List<Book> BookList = new();

    private string CurrentSearch = string.Empty;
    private EditContext SearchContext =  null!;

    protected override async Task OnInitializedAsync()
    {
        SearchContext = new EditContext(CurrentSearch);

        await BookService.FetchData();

        LoadDisplayBooks();
    }

    private void LoadDisplayBooks()
    {
        if (!string.IsNullOrWhiteSpace(CurrentSearch))
        {
            BookList = BookService.Books
                .Where(b => b.State == BookState.Read && (b.Title.Contains(CurrentSearch, StringComparison.InvariantCultureIgnoreCase) || b.SearchAuthor(CurrentSearch)))
                .OrderBy(m => m.Title)
                .ToList();
        }
        else
        {
            BookList = BookService.Books
                .Where(b => b.State == BookState.Read)
                .OrderBy(b => b.Title)
                .ToList();
        }

        StateHasChanged();
    }
}