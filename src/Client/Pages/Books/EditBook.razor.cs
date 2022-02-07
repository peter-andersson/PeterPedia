using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Pages.Books;

public partial class EditBook : ComponentBase
{
    [Inject]
    BookService BookService { get; set; } = null!;

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public Book? Book { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnClose { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnSuccess { get; set; }

    public bool IsTaskRunning { get; set; }

    private IList<Author> SelectedAuthor = new List<Author>();

    private readonly List<Author> Authors = new();

    private string Title
    {
        get
        {
            if (Book is null)
            {
                return "Add book";
            }

            if (Book.Id > 0)
            {
                return "Edit book";
            }
            else
            {
                return "Add book";
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await BookService.FetchData();

        Authors.Clear();
        Authors.AddRange(BookService.Authors);
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        SelectedAuthor.Clear();

        if (Book is null)
        {
            Book = new Book();
        }
        else
        {
            Book = await CreateCopy(Book);
        }        
    }

    private async Task<IEnumerable<Author>> GetAuthor(string searchText)
    {
        return await Task.FromResult(Authors.Where(x => x.Name.ToLower().Contains(searchText.ToLower())).ToList());
    }

    private Task<Author> ItemAddedMethod(string searchText)
    {
        var author = new Author
        {
            Name = searchText
        };
        Authors.Add(author);
        return Task.FromResult(author);
    }

    private async Task Save()
    {
        if (Book is null)
        {
            return;
        }

        IsTaskRunning = true;
        
        Book.Authors.Clear();
        foreach (var author in SelectedAuthor)
        {
            Book.Authors.Add(author.Name);
        }

        bool result;
        if (Book.Id > 0)
        {
            result = await BookService.Update(Book);
        }
        else
        {
            result = await BookService.Add(Book);
        }

        IsTaskRunning = false;
        if (result)
        {
            await OnSuccess.InvokeAsync();
        }
    }

    private async Task Cancel()
    {
        await OnClose.InvokeAsync();
    }

    public async Task<Book> CreateCopy(Book book)
    {
        var result = new Book()
        {
            Id = book.Id,
            State = book.State,
            Title = book.Title,
        };

        SelectedAuthor = new List<Author>();
        foreach (var author in book.Authors)
        {
            var authorData = await BookService.GetAuthor(author);
            if (authorData is not null)
            {
                result.Authors.Add(author);
                SelectedAuthor.Add(authorData);
            }
        }

        result.AuthorText = string.Join(", ", result.Authors);

        return result;
    }
}