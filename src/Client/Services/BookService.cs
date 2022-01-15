
using PeterPedia.Shared;
using System.Net.Http.Json;
using Blazored.Toast.Services;
using System.Text.Json;

namespace PeterPedia.Client.Services;

public class BookService
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    private static readonly PeterPediaJSONContext Context = new(Options);

    private readonly HttpClient _http;

    private readonly IToastService _toast;

    public BookService(HttpClient httpClient, IToastService toastService)
    {
        _http = httpClient;
        _toast = toastService;
    }

    public List<Author> Authors { get; private set; } = null!;

    public List<Book> Books { get; private set; } = null!;

    public async Task FetchData()
    {
        if (Authors is null)
        {
            await FetchAuthor();
        }

        if (Books is null)
        {
            await FetchBooks();
        }
    }

    public async Task FetchAuthor()
    {
        var authors = await _http.GetFromJsonAsync("/api/Author", Context.AuthorArray);

        if (authors is null)
        {
            return;
        }

        Authors = new List<Author>(authors.Length);
        Authors.AddRange(authors);

        Authors = Authors.OrderBy(a => a.Name).ToList();
    }

    public async Task FetchBooks()
    {
        var books = await _http.GetFromJsonAsync("/api/Book", Context.BookArray);

        if (books is null)
        {
            return;
        }

        Books = new List<Book>(books.Length);
        Books.AddRange(books);
    }

    public async Task<Book?> Get(int id)
    {
        if (Books is null)
        {
            await FetchBooks();
        }

        if (Books is not null)
        {
            return Books.Where(b => b.Id == id).FirstOrDefault();
        }
        else
        {
            return null;
        }
    }

    public async Task<Author?> GetAuthor(string name)
    {
        if (Authors is null)
        {
            await FetchAuthor();
        }

        if (Authors is not null)
        {
            return Authors.Where(a => a.Name.ToLowerInvariant() == name.ToLowerInvariant()).FirstOrDefault();
        }
        else
        {
            return null;
        }
    }

    public async Task<bool> Update(Book book)
    {
        if (book is null)
        {
            _toast.ShowError("Invalid book, can't update");
            return false;
        }

        var existingBook = await Get(book.Id);
        if (existingBook is null)
        {
            _toast.ShowError("Can't update a book that doesn't exist.");
            return false;
        }

        using var response = await _http.PutAsJsonAsync("/api/Book", book, Context.Book);

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess($"Book {book.Title} saved");

            existingBook.Title = book.Title;
            existingBook.State = book.State;
            existingBook.Authors.Clear();
            foreach (var author in book.Authors)
            {
                existingBook.Authors.Add(author);
            }

            await FetchAuthor();

            return true;
        }
        else
        {
            _toast.ShowError($"Failed to save book. StatusCode = {response.StatusCode}");
            return false;
        }
    }

    public async Task<bool> Add(Book? book)
    {
        if (book is null)
        {
            _toast.ShowError("Invalid book, can't add");
            return false;
        }

        using var response = await _http.PostAsJsonAsync("/api/Book", book, Context.Book);

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess($"Book {book.Title} added");

            book = await response.Content.ReadFromJsonAsync(Context.Book);

            if (book is not null)
            {
                Books.Add(book);
            }

            await FetchAuthor();

            return true;
        }
        else
        {
            _toast.ShowError($"Failed to save book. StatusCode = {response.StatusCode}");
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {
        var book = await Get(id);
        if (book is null)
        {
            _toast.ShowError($"{id} is not a valid book id. Can't remove book.");
            return false;
        }

        using var response = await _http.DeleteAsync($"/api/Book/{id}");

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess("Book deleted");

            Books.Remove(book);

            return true;
        }
        else
        {
            _toast.ShowSuccess($"Failed to delete book. StatusCode = {response.StatusCode}");

            return false;
        }
    }
}