using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Episodes;

public partial class ShowView : ComponentBase
{
    [Inject]
    private NavigationManager NavManager { get; set; } = null!;

    [Parameter]
    public Show Show { get; set; } = null!;
    
    private void OpenShow() => NavManager.NavigateTo($"/episodes/{Show.Id}");
}
