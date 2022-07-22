using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace Movies.App.Pages;

public partial class Edit : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    [Parameter]
    public string Id { get; set; } = null!;

    private string ErrorMessage { get; set; } = string.Empty;

    private EditMovie? Movie { get; set; }

    private bool IsSaveTaskRunning { get; set; }

    private bool IsDeleteTaskRunning { get; set; }

    private bool Loading { get; set; } = true;    

    protected override async Task OnInitializedAsync()
    {
        IsSaveTaskRunning = false;
        IsDeleteTaskRunning = false;
        ErrorMessage = string.Empty;

        try
        {
            Movie? movie = await Http.GetFromJsonAsync<Movie>($"/api/get/{Id}");

            if (movie is not null)
            {
                Movie = new EditMovie(movie);
            }
        }
        catch
        {
            Movie = null;
        }
        finally
        {
            Loading = false;
        }
    }

    public async Task SaveAsync()
    {
        ErrorMessage = string.Empty;

        if (Movie is null)
        {
            return;
        }

        IsSaveTaskRunning = true;

        try
        {
            var movie = new Movie()
            {
                Id = Movie.Id,
                Title = Movie.Title,
                WatchedDate = Movie.WatchedDate,
                Refresh = Movie.Refresh
            };

            HttpResponseMessage response = await Http.PostAsJsonAsync("/api/update", movie);

            if (response.IsSuccessStatusCode)
            {
                Navigation.NavigateBack();
            }
            else
            {
                ErrorMessage = "Failed to update movie.";
            }
        }
        catch
        {
            Movie = null;
        }
        finally
        {
            IsSaveTaskRunning = false;
        }
    }

    private async Task DeleteAsync()
    {
        ErrorMessage = string.Empty;

        if (Movie is null)
        {
            return;
        }

        try
        {
            IsDeleteTaskRunning = true;

            HttpResponseMessage response = await Http.DeleteAsync($"/api/delete/{Movie.Id}");

            if (response.IsSuccessStatusCode)
            {
                Navigation.NavigateBack();
            }
            else
            {
                ErrorMessage = "Failed to update movie.";
            }
        }
        catch
        {
            ErrorMessage = "Failed to delete movie.";
        }
        finally
        {
            IsDeleteTaskRunning = false;
        }
    }

    private void Close() => Navigation.NavigateBack();
}
