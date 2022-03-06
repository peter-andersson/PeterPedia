using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PeterPedia.Client.Pages.Books;

public partial class BookDialog : ComponentBase
{
    [Inject]
    private IBookManager BookManager { get; set; } = null!;

    [Inject]
    private IAuthorManager AuthorManager { get; set; } = null!;

    [CascadingParameter]
    public BlazoredModalInstance ModalInstance { get; set; } = null!;

    [Parameter]
    public Book Book { get; set; } = null!;

    private bool IsTaskRunning { get; set; }

    private BookModel EditBookModel { get; set; } = new();

    private string SubmitButtonText { get; set; } = string.Empty;

    private string Filter { get; set; } = string.Empty;

    private readonly List<Author> _authorList = new();

    private List<Author> AuthorList => _authorList.Where(a => a.Name.ToLower().Contains(Filter.ToLower())).OrderBy(a => a.Name).ToList();

    protected override async Task OnInitializedAsync()
    {
        IsTaskRunning = false;

        EditBookModel.Title = Book.Title;
        EditBookModel.State = Book.State;
        EditBookModel.SelectedAuthors.Clear();
        EditBookModel.SelectedAuthors.AddRange(Book.Authors);

        SubmitButtonText = Book.Id == 0 ? "Add" : "Save";

        List<Author> authors = await AuthorManager.GetAsync();

        _authorList.Clear();

        _authorList.AddRange(authors.ToList().Where(a => !EditBookModel.SelectedAuthors.Any(b => a.Id == b.Id)).ToList());
    }

    private async Task HandleValidSubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(EditBookModel.Title) ||
            (EditBookModel.SelectedAuthors.Count == 0))
        {
            return;
        }

        IsTaskRunning = true;

        Book.Title = EditBookModel.Title;
        Book.State = EditBookModel.State;
        Book.CoverUrl = EditBookModel.CoverUrl;
        Book.Authors.Clear();
        Book.Authors.AddRange(EditBookModel.SelectedAuthors);

        if (Book.Id == 0)
        {
            if (await BookManager.AddAsync(Book))
            {
                await ModalInstance.CloseAsync();
            }
        }
        else if (await BookManager.UpdateAsync(Book))
        {
            await ModalInstance.CloseAsync();
        }

        IsTaskRunning = false;
    }

    private async Task DeleteAsync()
    {
        IsTaskRunning = true;

        if (await BookManager.DeleteAsync(Book.Id))
        {
            await ModalInstance.CloseAsync();
        }

        IsTaskRunning = false;
    }

    private void RemoveAuthor(Author author)
    {
        _ = EditBookModel.SelectedAuthors.Remove(author);
        _authorList.Add(author);
    }

    private void FilterKeyPress(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            if (AuthorList.Count == 1)
            {
                EditBookModel.SelectedAuthors.Add(AuthorList[0]);
                _authorList.Remove(AuthorList[0]);
                Filter = string.Empty;
            }
        }
    }

    private async Task CloseAsync() => await ModalInstance.CancelAsync();
}
