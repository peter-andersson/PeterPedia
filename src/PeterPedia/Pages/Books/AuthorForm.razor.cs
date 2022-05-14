using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Books;

public partial class AuthorForm : ComponentBase
{
    [Inject]
    private IAuthorManager AuthorManager { get; set; } = null!;

    [Inject]
    private NavigationManager NavManager { get; set; } = null!;

    [Parameter]
    public int Id { get; set; }

    public Author Author { get; set; } = new();

    private bool IsTaskRunning { get; set; }

    private string SubmitButtonText { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        IsTaskRunning = false;

        if (Id > 0)
        {
            Result<Author> result = await AuthorManager.GetAsync(Id);

            if (result.Success)
            {
                Author = result.Data;
            }
        }
        else
        {
            Author = new();
        }

        SubmitButtonText = Author.Id == 0 ? "Add" : "Save";
    }

    private async Task HandleValidSubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(Author?.Name))
        {
            return;
        }

        IsTaskRunning = true;

        if (Author.Id == 0)
        {
            Result<Author> addResult = await AuthorManager.AddAsync(Author);

            if (addResult.Success)
            {
                NavManager.NavigateTo("/books/authors");
            }
        }
        else
        {
            Result<Author> updateResult = await AuthorManager.UpdateAsync(Author);

            if (updateResult.Success)
            {
                NavManager.NavigateTo("/books/authors");
            }
        }

        IsTaskRunning = false;
    }

    private async Task DeleteAsync()
    {
        IsTaskRunning = true;

        Result<Author> result = await AuthorManager.DeleteAsync(Author.Id);

        if (result.Success)
        {
            NavManager.NavigateTo("/books/authors");
        }

        IsTaskRunning = false;
    }

    private void Close() => NavManager.NavigateTo("/books/authors");
}
