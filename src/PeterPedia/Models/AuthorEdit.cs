using System.ComponentModel.DataAnnotations;

namespace PeterPedia.Models;

public class AuthorEdit
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name must be specified.")]
    [StringLength(100, ErrorMessage = "Name is too long.")]
    public string Name { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Date of birth must be specified.")]
    public DateOnly DateOfBirth { get; set; }
}
