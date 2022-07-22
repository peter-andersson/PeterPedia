using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace Reader.App.Pages;

public partial class Edit : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    [Parameter]
    public string Id { get; set; } = string.Empty;

    private Subscription? Subscription { get; set; }

    private bool IsSaveTaskRunning { get; set; }

    private bool IsDeleteTaskRunning { get; set; }

    private bool Loading { get; set; } = true;

    private string ErrorMessage { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        IsSaveTaskRunning = false;
        IsDeleteTaskRunning = false;
        Loading = true;

        Subscription = await Http.GetFromJsonAsync<Subscription?>($"/api/get/{Id}");

        Loading = false;
    }

    public async Task SaveAsync()
    {
        if (Subscription is null)
        {
            return;
        }

        IsSaveTaskRunning = true;
        ErrorMessage = string.Empty;

        try
        {            
            HttpResponseMessage response = await Http.PostAsJsonAsync("/api/update", Subscription);

            if (response.IsSuccessStatusCode)
            {
                Navigation.NavigateBack();
            }
            else
            {
                ErrorMessage = "Failed to update subscription.";
            }
        }
        catch
        {
            Subscription = null;
        }
        finally
        {
            IsSaveTaskRunning = false;
        }
    }

    private async Task DeleteAsync()
    {
        ErrorMessage = string.Empty;

        if (Subscription is null)
        {
            return;
        }

        try
        {
            IsDeleteTaskRunning = true;

            HttpResponseMessage response = await Http.DeleteAsync($"/api/delete/{Subscription.Id}");

            if (response.IsSuccessStatusCode)
            {
                Navigation.NavigateBack();
            }
            else
            {
                ErrorMessage = "Failed to delete subscription.";
            }
        }
        catch
        {
            ErrorMessage = "Failed to delete subscription.";
        }
        finally
        {
            IsDeleteTaskRunning = false;
        }
    }

    private void Close() => Navigation.NavigateBack();
}
