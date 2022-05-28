using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Data.Models;

[Table("error")]
public class ErrorEF
{
    public ErrorEF()
    {
        Timestamp = DateTime.UtcNow;
        Module = string.Empty;
        Error = string.Empty;
    }

    public int Id { get; set; }

    public string Module { get; set; }

    public string Error { get; set; }

    public DateTime Timestamp { get; set; }
}
