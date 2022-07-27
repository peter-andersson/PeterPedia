using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Episodes.App.Pages;

public partial class Search : ComponentBase
{
    [Inject]
    private ITVService Service { get; set; } = null!;

    private TVShow[] ShowList { get; set; } = Array.Empty<TVShow>();

    private SearchModel EditModel { get; set; } = new();

    private bool Searching { get; set; } = false;

    private bool Loading { get; set; } = false;

    private InputText? Input { get; set; }

    private QueryData Query { get; } = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (Input?.Element != null)
            {
                await Input.Element.Value.FocusAsync();
            }
        }
    }

    private async Task SearchAsync()
    {
        Searching = true;

        Query.Page = 0;
        Query.PageSize = 20;

        Query.Search = string.IsNullOrWhiteSpace(EditModel.Search) ? "%" : $"%{EditModel.Search}%";

        ShowList = await Service.GetAsync(Query);
        Searching = false;
    }

    private async Task ChangePageAsync(int page)
    {
        Loading = true;

        Query.Page = page;
        Query.PageSize = 20;

        ShowList = await Service.GetAsync(Query);
        Loading = false;
    }
}
