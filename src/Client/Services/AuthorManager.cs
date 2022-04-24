using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;

namespace PeterPedia.Client.Services;

public class AuthorManager : IAuthorManager
{
    private static readonly JsonSerializerOptions s_Options = new(JsonSerializerDefaults.Web);
    private static readonly PeterPediaJSONContext s_Context = new(s_Options);

    private readonly HttpClient _http;
    private readonly IToastService _toast;
    private readonly IJSRuntime _js;

    private readonly List<Author> _authors;

    public AuthorManager(HttpClient httpClient, IToastService toastService, IJSRuntime js)
    {
        _http = httpClient;
        _toast = toastService;
        _js = js;

        _authors = new List<Author>();
    }

    public event Action? AuthorChanged;

    public async Task<bool> AddAsync(Author author)
    {
        using HttpResponseMessage response = await _http.PostAsJsonAsync("/api/Author", author, s_Context.Author);

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess($"Author {author.Name} added");

            Author? serverAuthor = await response.Content.ReadFromJsonAsync(s_Context.Author);

            if (serverAuthor is not null)
            {
                _authors.Add(serverAuthor);

                AuthorChanged?.Invoke();

                await _js.InvokeVoidAsync("authorStore.put", serverAuthor);
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
        Author? author = Get(id);
        if (author is null)
        {
            _toast.ShowError($"{id} is not a valid author id. Can't remove author.");
            return false;
        }

        using HttpResponseMessage response = await _http.DeleteAsync($"/api/Author/{id}");

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess("Author deleted");

            _authors.Remove(author);

            AuthorChanged?.Invoke();

            await _js.InvokeVoidAsync("authorStore.delete", author.Id);

            return true;
        }
        else
        {
            _toast.ShowSuccess($"Failed to delete author. StatusCode = {response.StatusCode}");

            return false;
        }
    }

    public async Task<List<Author>> GetAsync()
    {
        if (_authors.Count == 0)
        {
            Author[] authors = await _js.InvokeAsync<Author[]>("authorStore.getAll");

            foreach(Author author in authors)
            {
                _authors.Add(author);
            }
        }

        if (_authors.Count == 0)
        {
            await RefreshAsync();
        }

        return _authors;
    }

    public async Task RefreshAsync()
    {
        if (await FetchChangedAuthorsAsync() || await FetchDeletedAuthorsAsync())
        {
            AuthorChanged?.Invoke();
        }
    }

    public async Task<bool> UpdateAsync(Author author)
    {      
        using HttpResponseMessage? response = await _http.PutAsJsonAsync("/api/Author", author, s_Context.Author);

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess($"Author {author.Name} saved");

            Author? serverAuthor = await response.Content.ReadFromJsonAsync(s_Context.Author);

            Author? existing = Get(author.Id);

            if (serverAuthor is not null)
            {
                if (existing is not null)
                {
                    existing.Name = serverAuthor.Name;
                    existing.DateOfBirth = serverAuthor.DateOfBirth;
                    existing.LastUpdated = serverAuthor.LastUpdated;

                    await _js.InvokeVoidAsync("authorStore.put", existing);
                }
                else
                {
                    _authors.Add(serverAuthor);

                    await _js.InvokeVoidAsync("authorStore.put", serverAuthor);
                }

                AuthorChanged?.Invoke();
            }

            return true;
        }
        else
        {
            _toast.ShowError($"Failed to save author. StatusCode = {response.StatusCode}");
            return false;
        }
    }

    private Author? Get(int id) => _authors.FirstOrDefault(a => a.Id == id);

    private async Task<bool> FetchChangedAuthorsAsync()
    {
        var changed = false;
        Author? mostRecentlyUpdated = await _js.InvokeAsync<Author>("authorStore.getFirstFromIndex", "lastUpdated", "prev");
        DateTime since = mostRecentlyUpdated?.LastUpdated ?? DateTime.MinValue;
        var json = await _http.GetStringAsync($"/api/Author?lastupdate={since:yyyyMMddHHmmss}");

        if (!string.IsNullOrWhiteSpace(json))
        {
            await _js.InvokeVoidAsync("authorStore.putAllFromJson", json);

            Author[]? authors = JsonSerializer.Deserialize(json, s_Context.AuthorArray);

            if (authors is not null)
            {
                foreach (Author author in authors)
                {
                    Author? existing = Get(author.Id);

                    if (existing is not null)
                    {
                        existing.Name = author.Name;
                        existing.DateOfBirth = author.DateOfBirth;
                        existing.LastUpdated = author.LastUpdated;
                    }
                    else
                    {
                        _authors.Add(author);
                        changed = true;
                    }
                }
            }
        }

        return changed;
    }

    private async Task<bool> FetchDeletedAuthorsAsync()
    {
        var changed = false;

        DeleteLog[] deleteLog = await _js.InvokeAsync<DeleteLog[]>("authorStore.getDeleted");
        DateTime since = DateTime.MinValue;
        if (deleteLog.Length > 0)
        {
            since = deleteLog[0].Deleted;
        }

        var json = await _http.GetStringAsync($"/api/Author/deleted?deleted={since:yyyyMMddHHmmss}");
        if (!string.IsNullOrWhiteSpace(json))
        {
            DeleteLog[]? deletions = JsonSerializer.Deserialize(json, s_Context.DeleteLogArray);

            if (deletions is not null && deletions.Length > 0)
            {
                DeleteLog? maxDeleteTime = null;

                foreach (DeleteLog deletion in deletions)
                {
                    Author? existing = Get(deletion.Id);
                    if (existing is not null)
                    {
                        await _js.InvokeVoidAsync("authorStore.delete", existing.Id);
                        _authors.Remove(existing);
                        changed = true;
                    }

                    if ((maxDeleteTime is null) || (maxDeleteTime.Deleted < deletion.Deleted))
                    {
                        maxDeleteTime = deletion;
                    }
                }

                if (maxDeleteTime is not null)
                {
                    await _js.InvokeVoidAsync("authorStore.putDeleted", maxDeleteTime);
                }
            }
        }

        return changed;
    }
}
