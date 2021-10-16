using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Server.Data.Models
{
    [Table("readlist")]
    public class ReadListEF
    {
        public int Id { get; set; }

        public string Url { get; set; } = null!;

        public DateTime Added { get; set; }
    }
}
