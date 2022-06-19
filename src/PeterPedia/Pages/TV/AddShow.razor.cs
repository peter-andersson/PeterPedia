using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages.TV;

public partial class AddShow : ComponentBase
{
    [Inject]
    private ITVShows TVShows { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    public bool IsTaskRunning { get; set; } = false;

    public NewShow Show { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public string SuccessMessage { get; set; } = string.Empty;

    public async Task AddAsync()
    {
        IsTaskRunning = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        var showRegex = new Regex("^https://www.themoviedb.org/tv/(\\d+)");

        if (!int.TryParse(Show.Url, out var showId))
        {
            if (showRegex.IsMatch(Show.Url ?? string.Empty))
            {
                Match? matches = showRegex.Match(Show.Url ?? string.Empty);

                if (!int.TryParse(matches.Groups[1].Value, out showId))
                {
                    ErrorMessage = "Can't add show, invalid show id.";
                }
            }
        }

        Result<string> result = await TVShows.AddAsync(showId);

        IsTaskRunning = false;

        if (result is SuccessResult<string> successResult)
        {
            Show = new NewShow();
            SuccessMessage = successResult.Data;
        }
        else
        {
            ErrorMessage = result is ErrorResult<string> errorResult ? errorResult.Message : "Unknown error";
        }
    }

    private void Close() => Navigation.NavigateBack();
}
