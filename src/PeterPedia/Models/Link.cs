using System.ComponentModel.DataAnnotations;

namespace PeterPedia.Models;

public class Link
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Url { get; set; } = string.Empty;
}
