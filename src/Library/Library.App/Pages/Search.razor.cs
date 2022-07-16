using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Library.App.Pages;

public partial class Search : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    private Book[] BookList { get; set; } = Array.Empty<Book>();

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
            IncludeRead = true,
            IncludeReading = true,
            IncludeWantToRead = true
        };

        try
        {
            HttpResponseMessage response = await Http.PostAsJsonAsync("/api/query", query);

            if (response.IsSuccessStatusCode)
            {
                BookList = await response.Content.ReadFromJsonAsync<Book[]>() ?? Array.Empty<Book>();
            }
        }
        catch
        {
            BookList = Array.Empty<Book>();
        }
        finally
        {
            Searching = false;
        }
    }

    public void OpenBook(Book book) => Navigation.NavigateTo($"/book/{book.Id}");
}
