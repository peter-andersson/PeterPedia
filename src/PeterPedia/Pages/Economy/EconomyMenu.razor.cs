using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Economy;

public partial class EconomyMenu : ComponentBase
{
    [Parameter]
    public Models.EconomyPage Page { get; set; } = Models.EconomyPage.None;

    private string LinkClass(Models.EconomyPage page) => page == Page ? "nav-link active" : "nav-link";
}
