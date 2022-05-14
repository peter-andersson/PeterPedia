using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Data.Models;

[Table("photo")]
public class PhotoEF
{
    public PhotoEF()
    {
        FileName = string.Empty;
        AbsolutePath = string.Empty;
    }

    public int Id { get; set; }

    public string FileName { get; set; }

    public string AbsolutePath { get; set; }

    public AlbumEF Album { get; set; } = null!;
}
