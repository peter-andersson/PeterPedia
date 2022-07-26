using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Episodes.App.Pages;

public partial class Search : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;

    private TVShow[] ShowList { get; set; } = Array.Empty<TVShow>();

    private string Filter { get; set; } = string.Empty;

    private bool Searching { get; set; } = false;

    private ElementReference? Input { get; set; }

    public async Task InputKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await QueryAsync();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (Input is not null)
            {
                await Input.Value.FocusAsync();
            }
        }
    }

    private async Task QueryAsync()
    {
        Searching = true;

        var query = new QueryData()
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
                ShowList = await response.Content.ReadFromJsonAsync<TVShow[]>() ?? Array.Empty<TVShow>();
            }
        }
        catch
        {
            ShowList = Array.Empty<TVShow>();
        }
        finally
        {
            Searching = false;
        }
    }
}
