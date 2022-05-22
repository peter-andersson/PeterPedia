namespace PeterPedia.Models;

public class Transaction
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string Note1 { get; set; } = string.Empty;

    public string Note2 { get; set; } = string.Empty;

    public double Amount { get; set; }

    public Category? Category { get; set; }
}
