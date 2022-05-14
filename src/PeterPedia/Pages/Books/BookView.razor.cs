using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Books;

public partial class BookView : ComponentBase
{
    [Inject]
    private NavigationManager NavManager { get; set; } = null!;

    [Parameter]
    public Book Book { get; set; } = null!;

    private void SelectBook(Book book) => NavManager.NavigateTo($"book/{book.Id}");
}
