namespace PeterPedia.Models;

public class CategorySummary
{
    public string Name { get; set; } = string.Empty;

    public double TotalAmount { get; set; }

    public CategorySummary? Parent { get; set; }

    public List<CategorySummary> Children { get; set; } = new();
}
