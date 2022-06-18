using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Library;

public partial class BookView : ComponentBase
{
    [Inject]
    private Navigation Navigation { get; set; } = null!;

    [Parameter]
    public Book Book { get; set; } = null!;

    private void SelectBook(Book book) => Navigation.NavigateTo($"/library/book/{book.Id}");
}
