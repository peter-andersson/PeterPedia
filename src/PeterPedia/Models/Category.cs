using System.Text;

namespace PeterPedia.Models;

public class Category
{
    public Category() => Name = string.Empty;

    public int Id { get; set; }

    public string Name { get; set; }

    public bool IgnoreInOverview { get; set; }

    public Category? Parent { get; set; }

    public int ParentId { get; set; }

    public string Display
    {
        get
        {
            var builder = new StringBuilder();

            builder.Insert(0, Name);

            AddParents(builder, Parent);

            return builder.ToString();
        }
    }

    private void AddParents(StringBuilder builder, Category? parent)
    {
        if (parent is null)
        {
            return;
        }

        builder.Insert(0, $"{parent.Name}:");

        AddParents(builder, parent.Parent);
    }
}
