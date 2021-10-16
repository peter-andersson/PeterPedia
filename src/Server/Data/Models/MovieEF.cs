using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Server.Data.Models
{
    [Table("movie")]
    public class MovieEF
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string ImdbId { get; set; } = null!;

        public string OriginalTitle { get; set; } = null!;

        [MaxLength(2)]
        public string OriginalLanguage { get; set; } = null!;

        [MaxLength(255)]
        public string Title { get; set; } = null!;

        public int? RunTime { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? ReleaseDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? WatchedDate { get; set; }
    }
}
