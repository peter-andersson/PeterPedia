using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Server.Data.Models
{
    [Table("author")]
    public class AuthorEF
    {
        private static readonly DateTime s_DefaultUpdateDate = new(2000, 1, 1);
        public AuthorEF() => LastUpdated = s_DefaultUpdateDate;

        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public DateOnly? DateOfBirth { get; set; }

        public DateTime LastUpdated { get; set; }

        public ICollection<BookEF> Books { get; set; } = null!;
    }
}
