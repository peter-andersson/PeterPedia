using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Movies;

public partial class Movies : ComponentBase, IDisposable
{
    [Inject]
    private IMovieManager MovieManager { get; set; } = null!;

    [CascadingParameter]
    private IModalService Modal { get; set; } = null!;

    private readonly List<Movie> _movies = new();

    public string Filter { get; set; } = string.Empty;

    public bool Loading { get; set; } = true;

    public List<Movie> AllMovies => _movies.Where(m => m.Search(Filter)).ToList();

    public List<Movie> WatchList => _movies.Where(m => !m.WatchedDate.HasValue).ToList();

    protected override async Task OnInitializedAsync()
    {
        MovieManager.MovieChanged += async () => await RefreshMoviesAsync();

        await RefreshMoviesAsync();

        Loading = false;
    }

    private async Task RefreshMoviesAsync()
    {
        List<Movie> movies = await MovieManager.GetAsync();

        _movies.Clear();
        _movies.AddRange(movies.OrderBy(a => a.Title).ToList());

        StateHasChanged();
    }

    public void AddMovie() => _ = Modal.Show<AddMovieDialog>("Add movie", new ModalOptions()
    {
        Class = "blazored-modal w-50",
    });

    private void SelectMovie(Movie movie)
    {
        var parameters = new ModalParameters();
        parameters.Add("Movie", movie);

        Modal.Show<MovieDialog>("Edit movie", parameters);
    }

    public void Dispose()
    {
        MovieManager.MovieChanged -= async () => await RefreshMoviesAsync();

        GC.SuppressFinalize(this);
    }    
}
