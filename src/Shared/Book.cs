namespace PeterPedia.Shared;

public enum BookState
{
    WantToRead = 1,
    Reading = 2,
    Read = 3,
}

public class Book
{
    public Book()
    {
        Authors = new List<string>();
        Title = string.Empty;
        AuthorText = string.Empty;
        CurrentAuthor = string.Empty;
    }

    public int Id { get; set; }

    public string Title { get; set; }

    public List<string> Authors { get; set; }

    public BookState State { get; set; }

    public bool SearchAuthor(string searchString)
    {
        foreach (string author in Authors)
        {
            if (author.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public string AuthorText { get; set; }

    public string CurrentAuthor { get; set; }
}