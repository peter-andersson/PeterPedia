using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Books;

public partial class BookView : ComponentBase
{
    [Parameter]
    public Book Book { get; set; } = null!;

    [Parameter]
    public EventCallback<Book> OnSelect { get; set; }

    private async Task SelectBookAsync(Book book) => await OnSelect.InvokeAsync(book);
}
