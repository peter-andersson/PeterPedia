namespace PeterPedia;

public class Photo
{
    public Photo()
    {
        Album = string.Empty;
        Url = string.Empty;
    }

    public string Album { get; set; }

    public string Url { get; set; }
}
