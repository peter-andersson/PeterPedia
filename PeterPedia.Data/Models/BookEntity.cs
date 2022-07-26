namespace PeterPedia.Data.Models;

public class BookEntity : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public bool Reading { get; set; }

    public bool Read { get; set; }

    public bool WantToRead { get; set; }

    public List<string> Authors { get; set; } = new();
}
