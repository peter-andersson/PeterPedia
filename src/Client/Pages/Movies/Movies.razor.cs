using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PeterPedia.Client.Pages.Movies;

public partial class Movies : ComponentBase
{
    private IJSObjectReference _module = null!;

    [Inject]
    private MovieService MovieService { get; set; } = null!;

    [Inject]
    private IJSRuntime JS { get; set; } = null!;    

    public List<Movie> MovieList { get; private set; } = null!;

    public bool Watchlist { get; set; } = true;

    public Movie? SelectedMovie { get; set; } = null;

    public string AddMovieElement { get; } = "add-movie-dialog";

    public string DeleteMovieElement { get; } = "delete-movie-dialog";

    public string EditMovieElement { get; } = "edit-movie-dialog";

    private string _currentFilter = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await FilterMovies(string.Empty);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/dialog.js");
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

    public async Task AddMovie()
    {
        await ShowDialog(AddMovieElement);
    }

    public async Task AddDialogClose()
    {
        await HideDialog(AddMovieElement);
    }

    public async Task AddDialogSuccess()
    {
        await HideDialog(AddMovieElement);

        await FilterMovies(_currentFilter);

        StateHasChanged();
    }

    public async Task DeleteMovie(Movie movie)
    {
        SelectedMovie = null;

        SelectedMovie = movie;

        await FilterMovies(_currentFilter);

        await ShowDialog(DeleteMovieElement);
    }

    public async Task DeleteDialogClose()
    {
        await HideDialog(DeleteMovieElement);
    }

    public async Task DeleteDialogSuccess()
    {
        await HideDialog(DeleteMovieElement);

        StateHasChanged();
    }

    public async Task EditMovie(Movie movie)
    {
        SelectedMovie = movie;

        await ShowDialog(EditMovieElement);
    }

    public async Task EditDialogClose()
    {
        await HideDialog(EditMovieElement);
    }

    public async Task EditDialogSuccess()
    {
        SelectedMovie = null;

        await HideDialog(EditMovieElement);

        await FilterMovies(_currentFilter);

        StateHasChanged();
    }


    private async Task ShowDialog(string element)
    {
        if (string.IsNullOrWhiteSpace(element))
        {
            return;
        }

        if (_module is not null)
        {
            await _module.InvokeVoidAsync("ShowDialog", element);
        }
    }

    private async Task HideDialog(string element)
    {
        if (string.IsNullOrWhiteSpace(element))
        {
            return;
        }

        if (_module is not null)
        {
            await _module.InvokeVoidAsync("HideDialog", element);
        }
    }
}