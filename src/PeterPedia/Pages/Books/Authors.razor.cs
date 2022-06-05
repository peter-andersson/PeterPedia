using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Books;

public partial class Authors : ComponentBase
{
    [Inject]
    private IAuthorManager AuthorManager { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    private readonly List<Author> _authorList = new();

    private List<Author> AuthorList => _authorList.Where(a => a.Name.ToLower().Contains(Filter.ToLower())).ToList();

    private string Filter { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        Result<IList<Author>> result = await AuthorManager.GetAllAsync();

        _authorList.Clear();

        if (result.Success)
        {
            _authorList.AddRange(result.Data);
        }
    }

    public void SelectAuthor(Author author) => Navigation.NavigateTo($"/books/author/{author.Id}");
}
