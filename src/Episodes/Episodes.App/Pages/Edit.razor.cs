using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace Episodes.App.Pages;

public partial class Edit : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    [Parameter]
    public string Id { get; set; } = null!;

    private string ErrorMessage { get; set; } = string.Empty;

    private TVShow? Show { get; set; }

    private bool IsSaveTaskRunning { get; set; }

    private bool IsDeleteTaskRunning { get; set; }

    private bool IsTaskRunning { get; set; }

    private bool Loading { get; set; } = true;

    private bool ShowAll { get; set; } = false;
    
    protected override async Task OnInitializedAsync()
    {
        IsSaveTaskRunning = false;
        IsDeleteTaskRunning = false;
        ErrorMessage = string.Empty;

        try
        {
            Show = await Http.GetFromJsonAsync<TVShow>($"/api/get/{Id}");
        }
        catch
        {
            Show = null;
        }
        finally
        {
            Loading = false;
        }
    }

    private void ToggleShowAll() => ShowAll = !ShowAll;

    private async Task SaveAsync()
    {
        ErrorMessage = string.Empty;

        if (Show is null)
        {
            return;
        }

        IsSaveTaskRunning = true;

        try
        {           
            HttpResponseMessage response = await Http.PostAsJsonAsync("/api/update", Show);

            if (response.IsSuccessStatusCode)
            {
                Navigation.NavigateBack();
            }
            else
            {
                ErrorMessage = "Failed to update tv show.";
            }
        }
        catch
        {
            Show = null;
        }
        finally
        {
            IsSaveTaskRunning = false;
        }
    }

    private async Task DeleteAsync()
    {
        ErrorMessage = string.Empty;

        if (Show is null)
        {
            return;
        }

        try
        {
            IsDeleteTaskRunning = true;

            HttpResponseMessage response = await Http.DeleteAsync($"/api/delete/{Show.Id}");

            if (response.IsSuccessStatusCode)
            {
                Navigation.NavigateBack();
            }
            else
            {
                ErrorMessage = "Failed to delete tv show.";
            }
        }
        catch
        {
            ErrorMessage = "Failed to delete tv show.";
        }
        finally
        {
            IsDeleteTaskRunning = false;
        }
    }

    private async Task WatchSeasonAsync(Season season)
    {
        ErrorMessage = string.Empty;

        if (Show is null)
        {
            return;
        }

        IsTaskRunning = true;

        foreach (Episode episode in season.Episodes)
        {
            episode.Watched = true;
        }

        await UpdateTVShowAsync();
    }

    private async Task UnwatchSeasonAsync(Season season)
    {
        ErrorMessage = string.Empty;

        if (Show is null)
        {
            return;
        }

        IsTaskRunning = true;

        foreach (Episode episode in season.Episodes)
        {
            episode.Watched = false;
        }

        await UpdateTVShowAsync();        
    }

    private async Task WatchEpisodeAsync(Episode episode)
    {
        ErrorMessage = string.Empty;

        if (Show is null)
        {
            return;
        }

        IsTaskRunning = true;

        episode.Watched = true;

        await UpdateTVShowAsync();
    }

    private async Task UnwatchEpisodeAsync(Episode episode)
    {
        ErrorMessage = string.Empty;

        if (Show is null)
        {
            return;
        }

        IsTaskRunning = true;

        episode.Watched = false;

        await UpdateTVShowAsync();
    }

    private async Task UpdateTVShowAsync()
    {
        try
        {
            HttpResponseMessage response = await Http.PostAsJsonAsync("/api/update", Show);
        }
        catch
        {
            ErrorMessage = "Failed to update tv show.";
        }
        finally
        {
            IsTaskRunning = false;
        }
    }

    private void Close() => Navigation.NavigateBack();
}
