namespace PeterPedia.Shared;

public class Article
{
    public Article()
    {
        Title = string.Empty;
        Url = string.Empty;
    }

    public int Id { get; set; }

    public string Title { get; set; }

    public string Url { get; set; }

    public DateTime PublishDate { get; set; }

    public DateTime? ReadDate { get; set; }
}