using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Episodes.App.Pages;

public partial class Add : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    private bool IsTaskRunning { get; set; } = false;

    private TVUrl TVShow { get; set; } = new();

    private string ErrorMessage { get; set; } = string.Empty;

    private string SuccessMessage { get; set; } = string.Empty;

    private InputText? Input { get; set; }

    public async Task AddAsync()
    {        
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        var id = TVShow.Id;
        if (id is null)
        {
            ErrorMessage = "No tv show specified";
            return;
        }

        IsTaskRunning = true;

        try
        {
            HttpResponseMessage response = await Http.PostAsync($"/api/add/{id}", null);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    SuccessMessage = "TV show addded";
                    break;
                case HttpStatusCode.Conflict:
                    ErrorMessage = "TV show already exists";
                    break;
                case HttpStatusCode.InternalServerError:
                    ErrorMessage = "Failed to add TV show";
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

    private void Close() => Navigation.NavigateBack();

    public class TVUrl
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
