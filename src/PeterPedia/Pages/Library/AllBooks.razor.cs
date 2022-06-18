using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Library;

public partial class AllBooks : ComponentBase
{
    [Inject]
    private ILibrary Library { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    private readonly List<Book> _bookList = new();

    public string Filter { get; set; } = string.Empty;
    
    public List<Book> Books => _bookList.Where(b => b.Search(Filter)).ToList();
   
    protected override async Task OnInitializedAsync()
    {
        Result<IList<Book>> result = await Library.GetBooksAsync();

        _bookList.Clear();

        if (result.Success)
        {
            _bookList.AddRange(result.Data);
        }
    }

    public void OpenBook(Book book) => Navigation.NavigateTo($"/library/book/{book.Id}");
}
