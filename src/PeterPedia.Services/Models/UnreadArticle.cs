namespace PeterPedia.Services.Models;

public class UnreadArticle
{
    public UnreadArticle()
    {
        Group = string.Empty;
        Articles = new List<Article>();
    }

    public string Group { get; set; }

    public List<Article> Articles { get; set; }
}
