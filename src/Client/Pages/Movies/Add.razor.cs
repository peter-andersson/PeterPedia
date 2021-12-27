using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PeterPedia.Client.Services;

namespace PeterPedia.Client.Pages.Movies;
public partial class Add : ComponentBase
{
    [Inject]
    private MovieService MovieService { get; set; } = null!;

    private ElementReference Input;

    private EditContext AddContext = null!;
    private bool IsTaskRunning;
    private string MovieUrl = null!;

    protected override void OnInitialized()
    {
        MovieUrl = string.Empty;
        IsTaskRunning = false;

        AddContext = new EditContext(MovieUrl);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Input.Id is not null)
        {
            await Input.FocusAsync();
        }
    }

    private async Task AddMovie()
    {
        IsTaskRunning = true;

        var result = await MovieService.Add(MovieUrl);
        IsTaskRunning = false;

        if (result)
        {
            MovieUrl = string.Empty;
        }
    }
}