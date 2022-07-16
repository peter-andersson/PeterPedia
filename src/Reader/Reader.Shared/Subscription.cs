namespace Reader.Shared;

public class Subscription
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public int UpdateIntervalMinute { get; set; }

    public string? Group { get; set; }
}
