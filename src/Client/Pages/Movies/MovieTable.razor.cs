using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Movies;

public partial class MovieTable : ComponentBase
{
    [Parameter]
    public List<PeterPedia.Shared.Movie> Movies { get; set; } = null!;

    [Parameter]
    public string ReturnUrl { get; set; } = null!;
}