using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PeterPedia.Client.Pages.Movies;

public partial class Movies : ComponentBase
{
    [Inject]
    private MovieService MovieService { get; set; } = null!;

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    private IJSObjectReference _module = null!;

    public List<Movie> MovieList { get; private set; } = null!;

    public bool Watchlist { get; set; } = true;

    public Movie? SelectedMovie { get; set; } = null;

    private string _currentFilter = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await FilterMovies(string.Empty);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/movies.js");
    }

    public async Task FilterMovies(string filter)
    {
        _currentFilter = filter;
        MovieList = await MovieService.GetMovies(_currentFilter, watchList: Watchlist);
    }    

    public async Task ToggleUnwatched()
    {
        Watchlist = !Watchlist;

        await FilterMovies(string.Empty);
    }

    public async Task DialogClose()
    {
        SelectedMovie = null;

        if (_module is not null)
        {
            await _module.InvokeVoidAsync("HideEditModal");
            await _module.InvokeVoidAsync("HideDeleteModal");
        }
    }

    public async Task DialogSuccess()
    {
        SelectedMovie = null;
        await FilterMovies(_currentFilter);

        if (_module is not null)
        {
            await _module.InvokeVoidAsync("HideEditModal");
            await _module.InvokeVoidAsync("HideDeleteModal");
        }

        StateHasChanged();

    }

    public async Task EditMovie(Movie movie)
    {
        SelectedMovie = movie;

        if (_module is not null)
        {
            await _module.InvokeVoidAsync("ShowEditModal");
        }
    }

    public async Task DeleteMovie(Movie movie)
    {
        SelectedMovie = movie;
        if (_module is not null)
        {
            await _module.InvokeVoidAsync("ShowDeleteModal");
        }       
    }
}