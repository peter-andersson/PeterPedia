using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Reader.App.Pages;

public partial class Add : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    public bool IsTaskRunning { get; set; } = false;

    public string NewSubscriptionUrl { get; set; } = string.Empty;

    public string SuccessMessage { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public async Task InputKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await AddAsync();
        }
    }

    public async Task AddAsync()
    {
        if (string.IsNullOrEmpty(NewSubscriptionUrl))
        {
            return;
        }

        SuccessMessage = string.Empty;
        ErrorMessage = string.Empty;
        IsTaskRunning = true;

        var newSubscription = new NewSubscription()
        {
            Url = NewSubscriptionUrl,
        };

        try
        {
            HttpResponseMessage response = await Http.PostAsJsonAsync("/api/add", newSubscription);

            var result = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(result))
            {
                SuccessMessage = "Subscription added";
                NewSubscriptionUrl = string.Empty;
            }
            else
            {
                ErrorMessage = result;
            }
        }
        finally
        {
            IsTaskRunning = false;
        } 
    }

    private void Close() => Navigation.NavigateBack();
}
