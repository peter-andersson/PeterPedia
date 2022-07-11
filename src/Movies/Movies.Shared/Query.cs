namespace Movies.Shared;

public class Query
{
    public string Search { get; set; } = string.Empty;

    public int PageSize { get; set; }

    public int Page { get; set; }
}
