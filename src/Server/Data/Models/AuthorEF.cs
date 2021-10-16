using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Server.Data.Models
{
    [Table("author")]
    public class AuthorEF
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public ICollection<BookEF> Books { get; set; } = null!;
    }
}
