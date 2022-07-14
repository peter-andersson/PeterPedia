using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace Library.App.Pages;

public partial class BookForm : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    [Parameter]
    public string Id { get; set; } = null!;

    private BookEdit Book { get; set; } = new();

    private bool Loading { get; set; } = true;

    private bool IsSaveTaskRunning { get; set; }

    private bool IsDeleteTaskRunning { get; set; }

    private string ErrorMessage { get; set; } = string.Empty;

    private bool ShowDelete { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        IsSaveTaskRunning = false;
        IsDeleteTaskRunning = false;
        ErrorMessage = string.Empty;

        try
        {
            Book? book = await Http.GetFromJsonAsync<Book>($"/api/get/{Id}");

            if (book is not null)
            {
                ShowDelete = true;
                Book = new BookEdit(book);
            }
            else
            {
                Id = Guid.NewGuid().ToString();
                Book = new BookEdit();
            } 
        }
        catch
        {
            Id = Guid.NewGuid().ToString();
            Book = new BookEdit();
        }
        finally
        {
            Loading = false;
        }
    }

    private async Task HandleValidSubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(Book.Title) ||
            string.IsNullOrWhiteSpace(Book.Authors))
        {
            return;
        }

        IsSaveTaskRunning = true;

        try
        {
            var book = new Book()
            {
                Id = Id,
                Title = Book.Title,
                CoverUrl = Book.CoverUrl
            };

            var authors = Book.Authors.Split(",");
            foreach (var author in authors)
            {
                book.Authors.Add(author.Trim());
            }

            book.Read = Book.State == BookState.HaveRead;
            book.Reading = Book.State == BookState.Reading;
            book.WantToRead = Book.State == BookState.ToRead;

            HttpResponseMessage response = await Http.PostAsJsonAsync("/api/upsert", book);

            if (response.IsSuccessStatusCode)
            {
                Navigation.NavigateBack();
            }
            else
            {
                ErrorMessage = "Failed to add/update book.";
            }
        }
        finally
        {
            IsSaveTaskRunning = false;
        }
    }

    private async Task DeleteAsync()
    {
        ErrorMessage = string.Empty;
      
        try
        {
            IsDeleteTaskRunning = true;

            HttpResponseMessage response = await Http.DeleteAsync($"/api/delete/{Id}");

            if (response.IsSuccessStatusCode)
            {
                Navigation.NavigateBack();
            }
            else
            {
                ErrorMessage = "Failed to delete book.";
            }
        }
        catch
        {
            ErrorMessage = "Failed to delete book.";
        }
        finally
        {
            IsDeleteTaskRunning = false;
        }
    }  

    private void Close() => Navigation.NavigateBack();
}
