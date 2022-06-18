using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Library;

public partial class AuthorForm : ComponentBase
{
    [Inject]
    private ILibrary Library { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    [Parameter]
    public int Id { get; set; }

    public AuthorEdit Author { get; set; } = new();

    public bool IsTaskRunning { get; set; }

    public string SubmitButtonText { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        IsTaskRunning = false;

        if (Id > 0)
        {
            Result<Author> result = await Library.GetAuthorAsync(Id);

            if (result.Success)
            {
                Author.Name = result.Data.Name;
                Author.DateOfBirth = result.Data.DateOfBirth;
            }
        }
        else
        {
            Author = new();
        }

        SubmitButtonText = Id > 0 ? "Save" : "Add";
    }

    private async Task HandleValidSubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(Author?.Name))
        {
            return;
        }

        IsTaskRunning = true;

        var author = new Author()
        {
            Id = Id,
            Name = Author.Name,
            DateOfBirth = Author.DateOfBirth
        };

        if (Id > 0)
        {
            Result<Author> updateResult = await Library.UpdateAuthorAsync(author);

            if (updateResult.Success)
            {
                Navigation.NavigateBack();
            }            
        }
        else
        {
            Result<Author> addResult = await Library.AddAuthorAsync(author);

            if (addResult.Success)
            {
                Navigation.NavigateBack();
            }
        }

        IsTaskRunning = false;
    }

    private async Task DeleteAsync()
    {
        IsTaskRunning = true;

        Result<Author> result = await Library.DeleteAuthorAsync(Id);

        if (result.Success)
        {
            Navigation.NavigateBack();
        }

        IsTaskRunning = false;
    }

    private void Close() => Navigation.NavigateBack();
}
