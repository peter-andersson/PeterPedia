using System.ComponentModel.DataAnnotations;

namespace Reader.App.Models;

public class EditModel
{
    [Required(AllowEmptyStrings = false)]
    public string? Title { get; set; }

    public string? Group { get; set; }

    [Range(60, 1440, ErrorMessage = "Update interval must be between 60 and 1440 minutes.")]
    public int UpdateIntervalMinute { get; set; }

    [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Specify a valid time in format HH:mm.")]
    public string? UpdateAt { get; set; }

    [Required]
    [Url]
    public string? Url { get; set; }
}
