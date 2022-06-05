using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Episodes;

public partial class ShowView : ComponentBase
{
    [Inject]
    private Navigation Navigation { get; set; } = null!;

    [Parameter]
    public Show Show { get; set; } = null!;
    
    private void OpenShow() => Navigation.NavigateTo($"/episodes/{Show.Id}");
}
