namespace Reader.Shared;

public class UnreadGroup
{
    public string Group { get; set; } = string.Empty;

    public List<UnreadItem> Items { get; set; } = new();
}
