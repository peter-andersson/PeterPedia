using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Books;

public partial class Authors : ComponentBase, IDisposable
{
    [Inject]
    private IAuthorManager AuthorManager { get; set; } = null!;

    [CascadingParameter]
    private IModalService Modal { get; set; } = null!;

    private readonly List<Author> _authorList = new();

    private List<Author> AuthorList => _authorList.Where(a => a.Name.ToLower().Contains(Filter.ToLower())).ToList();

    private string Filter { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        AuthorManager.AuthorChanged += async () => await RefreshAuthorsAsync();

        await RefreshAuthorsAsync();
    }

    private void AddAuthor()
    {
        var parameters = new ModalParameters();
        parameters.Add("Author", new Author());

        Modal.Show<AuthorDialog>("Add author", parameters);
    }

    private void SelectAuthor(Author author)
    {
        var parameters = new ModalParameters();
        parameters.Add("Author", author);

        Modal.Show<AuthorDialog>("Edit author", parameters);
    }

    private async Task RefreshAuthorsAsync()
    {
        List<Author> authors = await AuthorManager.GetAsync();

        _authorList.Clear();
        _authorList.AddRange(authors.OrderBy(a => a.Name).ToList());

        StateHasChanged();
    }

    public void Dispose()
    {
        AuthorManager.AuthorChanged -= async () => await RefreshAuthorsAsync();

        GC.SuppressFinalize(this);
    }
}
