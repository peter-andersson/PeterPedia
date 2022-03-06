using System.ComponentModel.DataAnnotations;

namespace PeterPedia.Client.Models;

public class BookModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title must be specified.")]
    public string? Title { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Must set state.")]
    public BookState State { get; set; }

    public string? CoverUrl { get; set; }

    public List<Author> SelectedAuthors { get; set; } = new List<Author>();
}
