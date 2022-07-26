using System.ComponentModel.DataAnnotations;

namespace Episodes.App.Models;

public class EditModel
{
    public EditModel()
    {
    }

    public EditModel(TVShow show)
    {
        Show = show;

        Title = show.Title;
        Source = show.Source;
    }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public bool Refresh { get; set; }

    public TVShow? Show { get; set; }

    public void SaveProperties()
    {
        if (Show is null)
        {
            return;
        }

        Show.Title = Title;
        Show.Source = Source;
        Show.Refresh = Refresh;
    }
}
