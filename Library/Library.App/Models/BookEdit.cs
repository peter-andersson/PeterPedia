using System.ComponentModel.DataAnnotations;

namespace Library.App.Models;

public enum BookState
{
    ToRead = 1,
    Reading = 2,
    HaveRead = 3,
}

public class BookEdit
{
    public BookEdit() => State = BookState.ToRead;

    public BookEdit(Book book)
    {
        Title = book.Title;
        Authors = string.Join(", ", book.Authors);

        if (book.Read)
        {
            State = BookState.HaveRead;
        }

        if (book.WantToRead)
        {
            State = BookState.ToRead;
        }

        if (book.Reading)
        {
            State = BookState.Reading;
        }
    }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Title must be specified.")]
    public string Title { get; set; } = string.Empty;

    public string Authors { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Must set state.")]
    public BookState State { get; set; }

    public string? CoverUrl { get; set; }
}
