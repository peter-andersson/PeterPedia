using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Pages.Reader;

public partial class AddSubscription : ComponentBase
{
    [Inject]
    private RSSService RSSService { get; set; } = null!;

    [Parameter]
    public string? Id { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnClose { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnSuccess { get; set; }

    public bool IsTaskRunning { get; set; } = false;

    private EditContext AddContext = null!;

    private string NewSubscriptionUrl = null!;

    protected override void OnInitialized()
    {
        IsTaskRunning = false;
        NewSubscriptionUrl = string.Empty;
        AddContext = new EditContext(NewSubscriptionUrl);
    }

    private async Task Add()
    {
        IsTaskRunning = true;

        await RSSService.AddSubscriptionAsync(NewSubscriptionUrl);
        IsTaskRunning = false;

        await OnSuccess.InvokeAsync();
    }

    private async Task Close()
    {
        await OnClose.InvokeAsync();
    }
}