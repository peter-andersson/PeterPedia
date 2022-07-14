namespace Library.Shared;

public class Query
{
    public string Search { get; set; } = string.Empty;

    public bool IncludeRead { get; set; }

    public bool IncludeReading { get; set; }

    public bool IncludeWantToRead { get; set; }
}
