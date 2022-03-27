using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PeterPedia.Client.Pages.Movies;

public partial class AddMovieDialog : ComponentBase
{
    [Inject]
    private IMovieManager MovieService { get; set; } = null!;

    [CascadingParameter]
    private BlazoredModalInstance ModalInstance { get; set; } = null!;

    public bool IsTaskRunning { get; set; } = false;

    public string MovieUrl { get; set; } = string.Empty;

    public async Task InputKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await AddAsync();
        }
    }

    public async Task AddAsync()
    {
        if (string.IsNullOrEmpty(MovieUrl))
        {
            return;
        }

        IsTaskRunning = true;

        var result = await MovieService.AddAsync(MovieUrl);
        IsTaskRunning = false;

        IsTaskRunning = true;
        
        IsTaskRunning = false;

        if (result)
        {
            MovieUrl = string.Empty;

            await ModalInstance.CloseAsync();
        }
    }

    private async Task CloseAsync() => await ModalInstance.CancelAsync();
}
