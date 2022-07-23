namespace Reader.App.Models;

public class AddResult
{
    public string ErrorMessage { get; set; } = string.Empty;

    public List<string> Urls { get; set; } = new List<string>();
}
