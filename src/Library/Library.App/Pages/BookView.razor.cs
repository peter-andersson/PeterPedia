using Microsoft.AspNetCore.Components;

namespace Library.App.Pages;

public partial class BookView : ComponentBase
{
    [Inject]
    private Navigation Navigation { get; set; } = null!;

    [Parameter]
    public Book Book { get; set; } = null!;

    private void SelectBook(Book book) => Navigation.NavigateTo($"/book/{book.Id}");
}
