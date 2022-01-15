using Microsoft.AspNetCore.Components;
using PeterPedia.Shared;

namespace PeterPedia.Client.Pages.Books;

public partial class BookView : ComponentBase
{
    [Parameter]
    public Book Book { get; set; } = null!;

    [Parameter]
    public string ReturnUrl { get; set; } = null!;
}