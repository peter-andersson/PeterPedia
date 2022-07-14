using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace Library.App.Pages;

public partial class Index : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;

    private Book[] _bookList = Array.Empty<Book>();

    private List<Book> Reading => _bookList.Where(b => b.Reading).ToList();

    private List<Book> ToBeRead => _bookList.Where(b => b.WantToRead).ToList();

    private bool Loading { get; set; } = true;
   
    protected override async Task OnInitializedAsync()
    {
        Loading = true;

        var query = new Query()
        {
            Search = string.Empty,
            IncludeReading = true,
            IncludeWantToRead = true
        };

        try
        {
            HttpResponseMessage response = await Http.PostAsJsonAsync("/api/query", query);

            if (response.IsSuccessStatusCode)
            {
                _bookList = await response.Content.ReadFromJsonAsync<Book[]>() ?? Array.Empty<Book>();
            }
        }
        catch
        {
            _bookList = Array.Empty<Book>();
        }
        finally
        {
            Loading = false;
        }
    }   
}
