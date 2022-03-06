using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Books;

public partial class AuthorDialog : ComponentBase
{
    [Inject]
    private IAuthorManager AuthorManager { get; set; } = null!;

    [CascadingParameter]
    private BlazoredModalInstance ModalInstance { get; set; } = null!;

    [Parameter]
    public Author Author { get; set; } = null!;

    private bool IsTaskRunning { get; set; }

    private AuthorModel EditAuthorModel { get; set; } = new();

    private string SubmitButtonText { get; set; } = string.Empty;

    protected override void OnInitialized()
    {
        IsTaskRunning = false;

        EditAuthorModel.Name = Author.Name;
        EditAuthorModel.DateOfBirth = Author.DateOfBirth.ToDateTime(TimeOnly.MinValue);

        SubmitButtonText = Author.Id == 0 ? "Add" : "Save";
    }

    private async Task HandleValidSubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(EditAuthorModel?.Name) ||
            !EditAuthorModel.DateOfBirth.HasValue)
        {
            return;
        }

        IsTaskRunning = true;

        Author.Name = EditAuthorModel.Name;
        Author.DateOfBirth = DateOnly.FromDateTime(EditAuthorModel.DateOfBirth.Value);

        if (Author.Id == 0)
        {
            if (await AuthorManager.AddAsync(Author))
            {
                await ModalInstance.CloseAsync();
            }
        }
        else if (await AuthorManager.UpdateAsync(Author))
        {
            await ModalInstance.CloseAsync();
        }

        IsTaskRunning = false;
    }

    private async Task DeleteAsync()
    {
        IsTaskRunning = true;

        if (await AuthorManager.DeleteAsync(Author.Id))
        {
            await ModalInstance.CloseAsync();
        }

        IsTaskRunning = false;
    }

    private async Task CloseAsync() => await ModalInstance.CancelAsync();
}
