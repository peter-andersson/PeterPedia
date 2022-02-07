using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PeterPedia.Client.Pages.Books;

public partial class Books : ComponentBase
{
    private IJSObjectReference _module = null!;

    [Inject]
    private BookService BookService { get; set; } = null!;

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    public List<Book> BookList { get; set; } = null!;

    public bool Reading { get; set; } = true;

    public bool WantToRead { get; set; } = false;

    public bool Read { get; set; } = false;

    public Book? SelectedBook { get; set; } = null;

    public string DeleteBookElement { get; } = "delete-book-dialog";

    public string EditBookElement { get; } = "edit-book-dialog";

    private string _currentFilter = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await FilterBooks(string.Empty);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/dialog.js");
    }

    public async Task FilterBooks(string filter)
    {
        _currentFilter = filter;

        await BookService.FetchData();

        List<Book> books = new();

        if (Reading)
        {
            books = BookService.Books.Where(b => b.State == BookState.Reading).ToList();
        }

        if (WantToRead)
        {
            IEnumerable<Book> tmp = BookService.Books.Where(b => b.State == BookState.WantToRead);

            books.AddRange(tmp);
        }

        if (Read)
        {
            IEnumerable<Book> tmp = BookService.Books.Where(b => b.State == BookState.Read);

            books.AddRange(tmp);
        }

        if (!string.IsNullOrWhiteSpace(filter))
        {
            books = books.Where(b => b.Title.Contains(_currentFilter, StringComparison.InvariantCultureIgnoreCase) || b.SearchAuthor(_currentFilter)).ToList();
        }

        BookList = books.OrderBy(b => b.Title).ToList();
    }

    public async Task ToggleReading()
    {
        Reading = !Reading;
        await FilterBooks(string.Empty);
    }

    public async Task ToggleWantToRead()
    {
        WantToRead = !WantToRead;
        await FilterBooks(string.Empty);
    }

    public async Task  ToggleRead()
    {
        Read = !Read;
        await FilterBooks(string.Empty);
    }

    public async Task AddBook()
    {
        SelectedBook = null;

        await ShowDialog(EditBookElement);
    }

    public async Task DeleteBook(Book book)
    {
        SelectedBook = book;

        await FilterBooks(_currentFilter);

        await ShowDialog(DeleteBookElement);
    }

    public async Task DeleteDialogClose()
    {
        await HideDialog(DeleteBookElement);
    }

    public async Task DeleteDialogSuccess()
    {
        await HideDialog(DeleteBookElement);

        StateHasChanged();
    }

    public async Task EditBook(Book book)
    {
        SelectedBook = book;

        await ShowDialog(EditBookElement);
    }

    public async Task EditDialogClose()
    {
        await HideDialog(EditBookElement);
    }

    public async Task EditDialogSuccess()
    {
        SelectedBook = null;

        await HideDialog(EditBookElement);

        await FilterBooks(_currentFilter);

        StateHasChanged();
    }


    private async Task ShowDialog(string element)
    {
        if (string.IsNullOrWhiteSpace(element))
        {
            return;
        }

        if (_module is not null)
        {
            await _module.InvokeVoidAsync("ShowDialog", element);
        }
    }

    private async Task HideDialog(string element)
    {
        if (string.IsNullOrWhiteSpace(element))
        {
            return;
        }

        if (_module is not null)
        {
            await _module.InvokeVoidAsync("HideDialog", element);
        }
    }
}