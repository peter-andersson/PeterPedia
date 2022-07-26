namespace PeterPedia.Data.Models;

public class ArticleEntity : BaseEntity
{
#pragma warning disable CA1822 // Mark members as static
    public string Type => "article";
#pragma warning restore CA1822 // Mark members as static

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Group { get; set; } = string.Empty;

    public string Subscription { get; set; } = string.Empty;

    public DateTime PublishDate { get; set; }

    public DateTime? ReadDate { get; set; }
}
