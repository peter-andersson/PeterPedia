using System.ComponentModel.DataAnnotations;

namespace PeterPedia.Models;

public class NewShow
{
    [Required]
    public string? Url { get; set; }
}
