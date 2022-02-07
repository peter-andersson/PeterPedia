using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Pages.Books;

public partial class DeleteBook : ComponentBase
{
    [Inject]
    private BookService BookService { get; set; } = null!;

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public Book? Book { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnClose { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnSuccess { get; set; }

    public bool IsTaskRunning { get; set; }

    protected override void OnInitialized()
    {
        IsTaskRunning = false;
    }

    private async Task Cancel()
    {
        await OnClose.InvokeAsync();
    }

    private async Task Delete()
    {
        IsTaskRunning = true;

        if (Book is null)
        {
            await OnClose.InvokeAsync();

            return;
        }
        
        await BookService.Delete(Book.Id);
                
        IsTaskRunning = false;

        await OnSuccess.InvokeAsync();
    }
}