using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Data.Models;

[Table("category")]
public class CategoryEF
{
    public CategoryEF()
    {
        Name = string.Empty;
        Children = new List<CategoryEF>();
        Transactions = new List<TransactionEF>();
    }

    public int Id { get; set; }

    public string Name { get; set; }

    public bool IgnoreInOverView { get; set; }

    public CategoryEF? Parent { get; set; }

    public ICollection<CategoryEF> Children { get; set; }

    public ICollection<TransactionEF> Transactions { get; private set; }
}
