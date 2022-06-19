using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PeterPedia.Pages.Library;

public partial class BookForm : ComponentBase
{
    [Inject]
    private ILibrary Library { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    private readonly List<Author> _authorList = new();

    [Parameter]
    public int Id { get; set; }

    public BookEdit Book { get; set; } = new();

    public bool IsTaskRunning { get; set; }

    public string SubmitButtonText { get; set; } = string.Empty;

    public string Filter { get; set; } = string.Empty;
   
    public List<Author> AuthorList => _authorList.Where(a => a.Name.ToLower().Contains(Filter.ToLower())).OrderBy(a => a.Name).ToList();

    protected override async Task OnInitializedAsync()
    {
        IsTaskRunning = false;

        if (Id > 0)
        {
            Result<Book> bookResult = await Library.GetBookAsync(Id);

            if (bookResult.Success)
            {
                Book.Title = bookResult.Data.Title;
                Book.State = bookResult.Data.State;
                Book.Authors.AddRange(bookResult.Data.Authors);
            }
        }
        else
        {
            Book = new();
        }
        

        SubmitButtonText = Id == 0 ? "Add" : "Save";

        Result<IList<Author>> authorResult = await Library.GetAuthorsAsync();

        _authorList.Clear();

        if (authorResult.Success)
        {
            _authorList.AddRange(authorResult.Data.ToList().Where(a => !Book.Authors.Any(b => a.Id == b.Id)).ToList());
        }        
    }

    private async Task HandleValidSubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(Book.Title) ||
            (Book.Authors.Count == 0))
        {
            return;
        }

        IsTaskRunning = true;

        var book = new Book()
        {
            Id = Id,
            Title = Book.Title,
            State = Book.State,
            CoverUrl = Book.CoverUrl
        };

        book.Authors.AddRange(Book.Authors);

        Result<Book> result;

        if (book.Id == 0)
        {
            result = await Library.AddBookAsync(book);            
        }
        else
        {
            result = await Library.UpdateBookAsync(book);
        }

        if (result.Success)
        {
            Navigation.NavigateBack();
        }

        IsTaskRunning = false;
    }

    private async Task DeleteAsync()
    {
        IsTaskRunning = true;

        Result<Book> result = await Library.DeleteBookAsync(Id);
        if (result.Success)
        {
            Navigation.NavigateBack();
        }
        
        IsTaskRunning = false;
    }

    private void RemoveAuthor(Author author)
    {
        _ = Book.Authors.Remove(author);
        _authorList.Add(author);
    }

    private void FilterKeyPress(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            if (AuthorList.Count == 1)
            {
                Book.Authors.Add(AuthorList[0]);
                _authorList.Remove(AuthorList[0]);
                Filter = string.Empty;
            }
        }
    }

    private void Close() => Navigation.NavigateBack();
}
