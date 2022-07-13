using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace Movies.App.Pages;

public partial class Add : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    public bool IsTaskRunning { get; set; } = false;

    public MovieUrl Movie { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public string SuccessMessage { get; set; } = string.Empty;

    public async Task AddAsync()
    {        
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        var id = Movie.Id;
        if (id is null)
        {
            ErrorMessage = "No movie specified";
            return;
        }

        IsTaskRunning = true;

        try
        {
            HttpResponseMessage response = await Http.PostAsync($"/api/add/{id}", null);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    SuccessMessage = "Movie addded";
                    break;
                case HttpStatusCode.Conflict:
                    ErrorMessage = "Movie already exists";
                    break;
                case HttpStatusCode.InternalServerError:
                    ErrorMessage = "Failed to add movie";
                    break;
            }
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
        finally
        {
            IsTaskRunning = false;
        }
    }

    private void Close() => Navigation.NavigateBack();

    public class MovieUrl
    {
        [Required]
        public string? Url { get; set; }

        public int? Id
        {
            get
            {
                var movieRegex = new Regex("^https://www.themoviedb.org/movie/(\\d+)");

                if (movieRegex.IsMatch(Url ?? string.Empty))
                {
                    Match? matches = movieRegex.Match(Url ?? string.Empty);

                    if (int.TryParse(matches.Groups[1].Value, out var movieId))
                    {
                        return movieId;
                    }
                }

                return null;
            }
        }
    }
}
