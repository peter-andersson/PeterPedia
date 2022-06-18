using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PeterPedia.Services.Models;

public enum BookState
{
    ToRead = 1,
    Reading = 2,
    HaveRead = 3,
}

public class Book
{
    public Book()
    {
        Authors = new List<Author>();
        Title = string.Empty;
    }

    public int Id { get; set; }

    // TODO: Move annotation to viewmodel?

    [Required(AllowEmptyStrings = false, ErrorMessage = "Title must be specified.")]
    public string Title { get; set; }

    public List<Author> Authors { get; set; }

    public DateTime LastUpdated { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Must set state.")]
    public BookState State { get; set; }

    public string? CoverUrl { get; set; }

    public string StateText => State switch
    {
        BookState.ToRead => "To be read",
        BookState.Reading => "Reading",
        BookState.HaveRead => "Have read",
        _ => string.Empty,
    };

    public string AuthorText
    {
        get
        {
            var text = new StringBuilder();
            var addSeperator = false;
            foreach (Author author in Authors)
            {
                text.Append(author.Name);

                if (addSeperator)
                {
                    text.Append(", ");
                }

                addSeperator = true;
            }

            return text.ToString();
        }
    }

    public bool Search(string searchString)
    {
        if (string.IsNullOrEmpty(searchString))
        {
            return true;
        }

        if (Title.Contains(searchString, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (Author author in Authors)
        {
            if (author.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }  
}
