using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Books;

public partial class Books : ComponentBase, IDisposable
{
    [Inject]
    private IBookManager BookManager { get; set; } = null!;

    [CascadingParameter]
    private IModalService Modal { get; set; } = null!;

    private string Filter { get; set; } = string.Empty;

    private readonly List<Book> _bookList = new();

    private List<Book> AllBooks => _bookList.Where(b => b.Search(Filter)).ToList();

    private List<Book> Reading => _bookList.Where(b => b.State == BookState.Reading).ToList();

    private List<Book> ToBeRead => _bookList.Where(b => b.State == BookState.ToRead).ToList();

    private ModalOptions _options = new()
    {
        Class = "blazored-modal w-75",
        ContentScrollable = true,
    };

    protected override async Task OnInitializedAsync()
    {
        BookManager.BookChanged += async () => await RefreshBooksAsync();

        await RefreshBooksAsync();
    }
    private async Task RefreshBooksAsync()
    {
        List<Book> books = await BookManager.GetAsync();

        _bookList.Clear();
        _bookList.AddRange(books.OrderBy(a => a.Title).ToList());

        StateHasChanged();
    }

    private void AddBook()
    {
        var parameters = new ModalParameters();
        parameters.Add("Book", new Book());

        Modal.Show<BookDialog>("Add book", parameters, _options);
    }

    private void SelectBook(Book book)
    {
        var parameters = new ModalParameters();
        parameters.Add("Book", book);

        Modal.Show<BookDialog>("Edit book", parameters, _options);
    }

    public void Dispose()
    {
        BookManager.BookChanged -= async () => await RefreshBooksAsync();

        GC.SuppressFinalize(this);
    }
}
