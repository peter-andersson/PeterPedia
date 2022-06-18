namespace PeterPedia.Services.Models;

public class Author
{
    public Author() => Name = string.Empty;

    public int Id { get; set; }

    public string Name { get; set; }

    public DateOnly DateOfBirth { get; set; }
}
