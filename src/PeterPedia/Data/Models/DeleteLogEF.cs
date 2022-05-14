using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Data.Models;

public enum DeleteType
{
    Author,

    Book,

    Episode,

    Movie,
}

[Table("delete")]
public class DeleteLogEF
{
    public int Id { get; set; }

    public int DataId { get; set; }

    public DateTime Deleted { get; set; }

    public DeleteType Type { get; set; }
}
