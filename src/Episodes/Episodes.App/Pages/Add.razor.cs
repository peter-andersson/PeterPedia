using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Episodes.App.Pages;

public partial class Add : ComponentBase
{
    [Inject]
    private ITVService Service { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    private bool IsTaskRunning { get; set; } = false;

    private TVUrl TVShow { get; set; } = new();

    private InputText? Input { get; set; }

    public async Task AddAsync()
    {
        var id = TVShow.Id;
        if (id is null)
        {
            ToastService.ShowError("No tv show specified");
            return;
        }

        IsTaskRunning = true;

        Result result = await Service.AddAsync(id);

        if (result.Success)
        {
            ToastService.ShowSuccess("TV show added");
        }
        else
        {
            ToastService.ShowError(result.ErrorMessage);
        }

        IsTaskRunning = false;
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

    public class TVUrl
    {
        [Required]
        public string? Url { get; set; }

        public int? Id
        {
            get
            {
                var movieRegex = new Regex("^https://www.themoviedb.org/tv/(\\d+)");

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
