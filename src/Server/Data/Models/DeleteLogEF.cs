using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Server.Data.Models
{
    public enum DeleteType
    {
        Author,

        Book,

        Movie,

        Show,
    }

    [Table("delete")]
    public class DeleteLogEF
    {
        public int Id { get; set; }

        public int DataId { get; set; }

        public DateTime Deleted { get; set; }

        public DeleteType Type { get; set; }
    }
}
