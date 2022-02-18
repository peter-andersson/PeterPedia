namespace PeterPedia.Shared;

public class Subscription
{
    public Subscription()
    {
        Articles = new List<Article>();
        Title = string.Empty;
        Url = string.Empty;
    }

    public int Id { get; set; }

    public string Title { get; set; }

    public string? Group { get; set; }

    public string Url { get; set; }

    public int UpdateIntervalMinute { get; set; }

    public DateTime LastUpdate { get; set; }

    public List<Article> Articles { get; set; }
}