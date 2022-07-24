using System.ComponentModel.DataAnnotations;

namespace Reader.App.Models;

public class AddModel
{
    [Required(AllowEmptyStrings = false)]
    [Url]
    public string? Url { get; set; }
}
