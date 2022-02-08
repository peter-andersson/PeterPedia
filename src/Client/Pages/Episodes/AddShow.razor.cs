using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Pages.Episodes;

public partial class AddShow : ComponentBase
{
    [Inject]
    private TVService TVService { get; set; } = null!;

    [Parameter]
    public string? Id { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnClose { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnSuccess { get; set; }

    public bool IsTaskRunning { get; set; } = false;

    public string TVUrl { get; set; } = string.Empty;

    public async Task InputKeyDown(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await Add();
        }
    }

    public async Task Close()
    {
        await OnClose.InvokeAsync();
    }

    private async Task Add()
    {
        IsTaskRunning = true;

        var result = await TVService.Add(TVUrl);
        IsTaskRunning = false;
        if (result)
        {
            TVUrl = string.Empty;

            await OnSuccess.InvokeAsync();
        }
    }
}