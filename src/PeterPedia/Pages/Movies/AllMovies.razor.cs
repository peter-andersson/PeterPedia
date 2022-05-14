using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PeterPedia.Pages.Movies;

public partial class AllMovies : ComponentBase
{
    [Inject]
    private IMovieManager MovieManager { get; set; } = null!;

    public List<Movie> MovieList { get; set; } = new();

    public string Filter { get; set; } = string.Empty;

    public List<Movie> FilterMovieList { get; set; } = new();

    public bool NotFound { get; set; } = false;

    protected override async Task OnInitializedAsync() => MovieList.AddRange(await MovieManager.GetAllAsync());

    public void InputKeyDown(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            FilterMovieList.Clear();
            if (!string.IsNullOrWhiteSpace(Filter))
            { 
                FilterMovieList.AddRange(MovieList.Where(m => m.Search(Filter)).ToList());
            } 
            
            NotFound = FilterMovieList.Count == 0;
        }
    }
}
