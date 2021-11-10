namespace PeterPedia.Client.Common;

public class MenuItem
{
    public string Display { get; set; } = null!;

    public string Url { get; set; } = null!;

    public IconType IconType { get; set; } = IconType.None;
}