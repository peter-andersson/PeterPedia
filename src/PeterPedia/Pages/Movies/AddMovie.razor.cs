using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.Movies;

public partial class AddMovie : ComponentBase
{
    [Inject]
    private IMovieManager MovieManager { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    public bool IsTaskRunning { get; set; } = false;

    public NewMovie Movie { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public string SuccessMessage { get; set; } = string.Empty;

    public async Task AddAsync()
    {
        IsTaskRunning = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        var movieRegex = new Regex("^https://www.themoviedb.org/movie/(\\d+)");

        if (!int.TryParse(Movie.Url, out var movieId))
        {
            if (movieRegex.IsMatch(Movie.Url ?? string.Empty))
            {
                Match? matches = movieRegex.Match(Movie.Url ?? string.Empty);

                if (!int.TryParse(matches.Groups[1].Value, out movieId))
                {
                    ErrorMessage = "Can't add movie, invalid movie id.";
                }
            }
        }

        Result<string> result = await MovieManager.AddAsync(movieId);

        IsTaskRunning = false;

        if (result is SuccessResult<string> successResult)
        {
            Movie = new NewMovie();
            SuccessMessage = successResult.Data;
        }
        else
        {
            ErrorMessage = result is ErrorResult<string> errorResult ? errorResult.Message : "Unknown error";
        }
    }

    private void Close() => Navigation.NavigateBack();
}
