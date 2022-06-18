using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Library;

public partial class Authors : ComponentBase
{
    [Inject]
    private ILibrary Library { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    private readonly List<Author> _authorList = new();

    public List<Author> AuthorList => _authorList.Where(a => a.Name.ToLower().Contains(Filter.ToLower())).ToList();

    public string Filter { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        Result<IList<Author>> result = await Library.GetAuthorsAsync();

        _authorList.Clear();

        if (result.Success)
        {
            _authorList.AddRange(result.Data);
        }
    }

    public void OpenAuthor(Author author) => Navigation.NavigateTo($"/library/author/{author.Id}");
}
