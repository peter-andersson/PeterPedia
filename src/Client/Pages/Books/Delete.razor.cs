using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Services;

namespace PeterPedia.Client.Pages.Books;

public partial class Delete : ComponentBase
{
    [Inject]
    private BookService BookService { get; set; } = null!;

    [Inject]
    private NavigationManager NavManager { get; set; } = null!;

    [Parameter]
    public int Id { get; set; }

    [Parameter]
    public string ReturnUrl { get; set; } = string.Empty;

    private string Title = string.Empty;

    private bool IsTaskRunning;

    protected override async Task OnInitializedAsync()
    {
        IsTaskRunning = false;
        var book = await BookService.Get(Id);

        ReturnUrl ??= "";

        if (book is null)
        {
            NavManager.NavigateTo(ReturnUrl);
        }
        else
        {
            Title = book.Title;
        }
    }

    private void Cancel()
    {
        NavManager.NavigateTo(ReturnUrl);
    }

    private async Task DeleteBook()
    {
        IsTaskRunning = true;

        var result = await BookService.Delete(Id);

        IsTaskRunning = false;
        if (result)
        {
            NavManager.NavigateTo(ReturnUrl);
        }
    }
}