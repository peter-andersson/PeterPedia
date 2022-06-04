using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Data.Models;

[Table("album")]
public class AlbumEF
{
    public AlbumEF()
    {
        Name = string.Empty;

        Photos = new List<PhotoEF>();
    }

    public int Id { get; set; }

    public string Name { get; set; }

    public IList<PhotoEF> Photos { get; private set; }
}
