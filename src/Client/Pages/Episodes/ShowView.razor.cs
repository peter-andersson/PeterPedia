using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Episodes;

public partial class ShowView : ComponentBase
{
    [Parameter]
    public Show Show { get; set; } = null!;

    [Parameter]
    public EventCallback<Show> OnSelect { get; set; }

    private async Task SelectShowAsync(Show movie) => await OnSelect.InvokeAsync(movie);
}
