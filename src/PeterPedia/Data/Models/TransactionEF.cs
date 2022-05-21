using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Data.Models;

[Table("transaction")]
public class TransactionEF
{
    public TransactionEF()
    {
        Note1 = string.Empty;
        Note2 = string.Empty;
    }

    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string Note1 { get; set; }

    public string Note2 { get; set; }

    public double Amount { get; set; }

    public CategoryEF? Category { get; private set; }
}
