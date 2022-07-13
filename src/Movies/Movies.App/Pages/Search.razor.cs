using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Movies.App.Pages;

public partial class Search : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;

    public Movie[] MovieList { get; set; } = Array.Empty<Movie>();

    public string Filter { get; set; } = string.Empty;

    public async Task InputKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await QueryAsync();
        }
    }

    private async Task QueryAsync()
    {
        var query = new Query()
        {
            Search = Filter,
            Page = 0,
            PageSize = 50
        };

        try
        {
            HttpResponseMessage response = await Http.PostAsJsonAsync("/api/query", query);

            if (response.IsSuccessStatusCode)
            {
                MovieList = await response.Content.ReadFromJsonAsync<Movie[]>() ?? Array.Empty<Movie>();
            }
        }
        catch
        {
            MovieList = Array.Empty<Movie>();
        }
    }
}
