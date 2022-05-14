using System.ComponentModel.DataAnnotations;

namespace PeterPedia.Models;

public class NewMovie
{
    [Required]
    public string? Url { get; set; }
}
