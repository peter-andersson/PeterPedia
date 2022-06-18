using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Library;

public partial class Books : ComponentBase
{
    [Inject]
    private ILibrary Library { get; set; } = null!;

    private readonly List<Book> _bookList = new();

    public List<Book> Reading => _bookList.Where(b => b.State == BookState.Reading).ToList();

    public List<Book> ToBeRead => _bookList.Where(b => b.State == BookState.ToRead).ToList();
   
    protected override async Task OnInitializedAsync()
    {
        Result<IList<Book>> result = await Library.GetBooksAsync();

        _bookList.Clear();

        if (result.Success)
        {
            _bookList.AddRange(result.Data);
        }
    }   
}
