using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace Movies.App.Pages;

public partial class Index : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;

    private Movie[] MovieList { get; set; } = Array.Empty<Movie>();

    private bool Loading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        MovieList = await Http.GetFromJsonAsync<Movie[]>("/api/watchlist") ?? Array.Empty<Movie>();

        Loading = false;
    }
}
