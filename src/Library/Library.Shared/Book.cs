namespace Library.Shared;

public class Book
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public bool Reading { get; set; }

    public bool Read { get; set; }

    public bool WantToRead { get; set; }

    public List<string> Authors { get; set; } = new();

    public string? CoverUrl { get; set; } = string.Empty;

    public string AuthorText => string.Join(", ", Authors);
}
