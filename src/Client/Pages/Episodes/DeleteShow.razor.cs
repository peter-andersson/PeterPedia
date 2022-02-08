using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Pages.Episodes;

public partial class DeleteShow : ComponentBase
{
    [Inject]
    private TVService TVService { get; set; } = null!;

    [Parameter]
    public string? Id { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnClose { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnSuccess { get; set; }

    [Parameter, AllowNull]
    public Show? Show { get; set; }

    public bool IsTaskRunning { get; set; } = false;
   
    private async Task Cancel()
    {
        await OnClose.InvokeAsync();
    }

    private async Task Delete()
    {
        if (Show is null)
        {
            await OnClose.InvokeAsync();
            return;
        }

        IsTaskRunning = true;

        await TVService.Delete(Show.Id);

        IsTaskRunning = false;

        await OnSuccess.InvokeAsync();        
    }
}