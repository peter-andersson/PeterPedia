using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Books;

public partial class Books : ComponentBase
{
    [Inject]
    private IBookManager BookManager { get; set; } = null!;

    private string Filter { get; set; } = string.Empty;

    private readonly List<Book> _bookList = new();

    private List<Book> AllBooks => _bookList.Where(b => b.Search(Filter)).ToList();

    private List<Book> Reading => _bookList.Where(b => b.State == BookState.Reading).ToList();

    private List<Book> ToBeRead => _bookList.Where(b => b.State == BookState.ToRead).ToList();
   
    protected override async Task OnInitializedAsync()
    {
        Result<IList<Book>> result = await BookManager.GetAllAsync();

        _bookList.Clear();

        if (result.Success)
        {
            _bookList.AddRange(result.Data);
        }
    }   
}
