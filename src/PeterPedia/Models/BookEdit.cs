using System.ComponentModel.DataAnnotations;

namespace PeterPedia.Models;

public class BookEdit
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title must be specified.")]
    public string Title { get; set; } = string.Empty;

    public List<Author> Authors { get; set; } = new();

    [Required(AllowEmptyStrings = false, ErrorMessage = "Must set state.")]
    public BookState State { get; set; }

    public string? CoverUrl { get; set; }
}
