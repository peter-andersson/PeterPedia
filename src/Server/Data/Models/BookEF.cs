using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Server.Data.Models
{
    [Table("book")]
    public class BookEF
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public int State { get; set; }

        public DateTime LastUpdated { get; set; }

        public ICollection<AuthorEF> Authors { get; set; } = null!;
    }
}
