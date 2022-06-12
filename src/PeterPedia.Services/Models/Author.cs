using System.ComponentModel.DataAnnotations;

namespace PeterPedia.Services.Models;

public class Author
{
    public Author() => Name = string.Empty;

    public int Id { get; set; }

    // TODO: Move annotation to viewmodel?

    [Required(AllowEmptyStrings = false, ErrorMessage = "Name must be specified.")]
    [StringLength(100, ErrorMessage = "Name is too long.")]
    public string Name { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Date of birth must be specified.")]
    public DateOnly DateOfBirth { get; set; }

    public DateTime LastUpdated { get; set; }
}
