using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PeterPedia.Client.Services;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Pages.Movies;

public partial class AddMovie : ComponentBase
{
    [Inject]
    private MovieService MovieService { get; set; } = null!;

    [Parameter, AllowNull]
    public EventCallback<string> OnClose { get; set; }

    [Parameter, AllowNull]
    public EventCallback<string> OnSuccess { get; set; }

    public ElementReference Input;

    public bool IsTaskRunning { get; set; } = false;

    public string MovieUrl { get; set; } = string.Empty;

    protected override void OnInitialized()
    {           
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {                
        if (Input.Id is not null)
        {
            await Input.FocusAsync();
        }
    }

    public async Task InputKeyDown(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await AddMovieButton();
        }

    }

    public async Task AddMovieButton()
    {
        if (string.IsNullOrEmpty(MovieUrl))
        {
            return;
        }

        IsTaskRunning = true;

        var result = await MovieService.Add(MovieUrl);
        IsTaskRunning = false;

        if (result)
        {
            MovieUrl = string.Empty;

            await OnSuccess.InvokeAsync();
        }
    }

    public async Task Close()
    {
        await OnClose.InvokeAsync();
    }
}