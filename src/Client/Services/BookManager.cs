using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;

namespace PeterPedia.Client.Services;

public class BookManager : IBookManager
{
    private static readonly JsonSerializerOptions s_Options = new(JsonSerializerDefaults.Web);
    private static readonly PeterPediaJSONContext s_Context = new(s_Options);

    private readonly HttpClient _http;

    private readonly IToastService _toast;
    private readonly IJSRuntime _js;
    private readonly List<Book> _books;

    public BookManager(HttpClient httpClient, IToastService toastService, IJSRuntime js)
    {
        _http = httpClient;
        _toast = toastService;
        _js = js;

        _books = new List<Book>();
    }

    public event Action? BookChanged;

    public async Task<bool> AddAsync(Book book)
    {
        using HttpResponseMessage response = await _http.PostAsJsonAsync("/api/Book", book, s_Context.Book);

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess($"Book {book.Title} added");

            Book? serverBook = await response.Content.ReadFromJsonAsync(s_Context.Book);

            if (serverBook is not null)
            {
                _books.Add(serverBook);

                BookChanged?.Invoke();

                await _js.InvokeVoidAsync("bookStore.put", serverBook);
            }

            return true;
        }
        else
        {
            _toast.ShowError($"Failed to save book. StatusCode = {response?.StatusCode}");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Book? book = Get(id);
        if (book is null)
        {
            _toast.ShowError($"{id} is not a valid book id. Can't remove book.");
            return false;
        }

        using HttpResponseMessage response = await _http.DeleteAsync($"/api/Book/{id}");

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess("Book deleted");

            _books.Remove(book);

            BookChanged?.Invoke();

            await _js.InvokeVoidAsync("bookStore.delete", book.Id);

            return true;
        }
        else
        {
            _toast.ShowSuccess($"Failed to delete book. StatusCode = {response.StatusCode}");

            return false;
        }
    }

    public async Task<List<Book>> GetAsync()
    {
        if (_books.Count == 0)
        {
            Book[] books = await _js.InvokeAsync<Book[]>("bookStore.getAll");

            foreach (Book book in books)
            {
                _books.Add(book);
            }
        }

        if (_books.Count == 0)
        {
            await RefreshAsync();
        }

        return _books;
    }

    public async Task RefreshAsync()
    {
        if (await FetchChangedBooksAsync() || await FetchDeletedBooksAsync())
        {
            BookChanged?.Invoke();
        }
    }    

    public async Task<bool> UpdateAsync(Book book)
    {        
        using HttpResponseMessage response = await _http.PutAsJsonAsync("/api/Book", book, s_Context.Book);

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess($"Book {book.Title} saved");

            Book? serverBook = await response.Content.ReadFromJsonAsync(s_Context.Book);

            Book? existing = Get(book.Id);

            if (serverBook is not null)
            {
                if (existing is not null)
                {
                    existing.Title = serverBook.Title;
                    existing.State = serverBook.State;
                    existing.Authors = serverBook.Authors;
                    existing.LastUpdated = serverBook.LastUpdated;
                 
                    await _js.InvokeVoidAsync("bookStore.put", existing);
                }
                else
                {
                    _books.Add(serverBook);

                    await _js.InvokeVoidAsync("bookStore.put", serverBook);
                }

                BookChanged?.Invoke();
            }

            return true;
        }
        else
        {
            _toast.ShowError($"Failed to save book. StatusCode = {response.StatusCode}");
            return false;
        }
    }

    private async Task<bool> FetchChangedBooksAsync()
    {
        Book? mostRecentlyUpdated = await _js.InvokeAsync<Book>("bookStore.getFirstFromIndex", "lastUpdated", "prev");
        DateTime since = mostRecentlyUpdated?.LastUpdated ?? DateTime.MinValue;
        var json = await _http.GetStringAsync($"/api/Book?lastupdate={since:yyyyMMddHHmmss}");
        var changed = false;
        if (!string.IsNullOrWhiteSpace(json))
        {
            await _js.InvokeVoidAsync("bookStore.putAllFromJson", json);

            Book[]? books = JsonSerializer.Deserialize(json, s_Context.BookArray);

            if (books is not null)
            {
                foreach (Book book in books)
                {
                    Book? existing = Get(book.Id);

                    if (existing is not null)
                    {
                        existing.Title = book.Title;
                        existing.State = book.State;
                        existing.Authors = book.Authors;
                        existing.LastUpdated = book.LastUpdated;

                        changed = true;
                    }
                    else
                    {
                        _books.Add(book);

                        changed = true;
                    }
                }
            }
        }

        return changed;        
    }

    private async Task<bool> FetchDeletedBooksAsync()
    {
        var changed = false;

        DeleteLog[] deleteLog = await _js.InvokeAsync<DeleteLog[]>("bookStore.getDeleted");
        DateTime since = DateTime.MinValue;
        if (deleteLog.Length > 0)
        {
            since = deleteLog[0].Deleted;
        }

        var json = await _http.GetStringAsync($"/api/Book/deleted?deleted={since:yyyyMMddHHmmss}");
        if (!string.IsNullOrWhiteSpace(json))
        {
            DeleteLog[]? deletions = JsonSerializer.Deserialize(json, s_Context.DeleteLogArray);

            if (deletions is not null && deletions.Length > 0)
            {
                DeleteLog? maxDeleteTime = null;

                foreach (DeleteLog deletion in deletions)
                {
                    Book? existing = Get(deletion.Id);
                    if (existing is not null)
                    {
                        await _js.InvokeVoidAsync("bookStore.delete", existing.Id);
                        _books.Remove(existing);
                        changed = true;
                    }

                    if ((maxDeleteTime is null) || (maxDeleteTime.Deleted < deletion.Deleted))
                    {
                        maxDeleteTime = deletion;
                    }
                }

                if (maxDeleteTime is not null)
                {
                    await _js.InvokeVoidAsync("bookStore.putDeleted", maxDeleteTime);
                }
            }
        }

        return changed;
    }

    private Book? Get(int id) => _books.FirstOrDefault(b => b.Id == id);   
}
