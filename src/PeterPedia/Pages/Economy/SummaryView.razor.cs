using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Economy;

public partial class SummaryView : ComponentBase
{
    [Parameter]
    public CategorySummary Summary { get; set; } = null!;

    private string Class { get; set; } = "collapse";

    private void Toggle() => Class = Class == "collapse" ? "show" : "collapse";
}
