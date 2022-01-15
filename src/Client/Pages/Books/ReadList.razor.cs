using Microsoft.AspNetCore.Components;
using PeterPedia.Shared;
using PeterPedia.Client.Services;

namespace PeterPedia.Client.Pages.Books;

public partial class ReadList : ComponentBase
{
    [Inject]
    BookService BookService { get; set; } = null!;

    private List<Book> BookList = new();

    protected override async Task OnInitializedAsync()
    {
        await BookService.FetchData();

        BookList = BookService.Books.Where(b => b.State == BookState.WantToRead).OrderBy(b => b.Title).ToList();
    }
}