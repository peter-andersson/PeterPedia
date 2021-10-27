using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Server.Data.Models
{
    [Table("video")]
    public class VideoEF
    {
        public VideoEF()
        {
            Directory = string.Empty;
            Title = string.Empty;
            FileName = string.Empty;
            Type = string.Empty;
            AbsolutePath = string.Empty;
        }

        public int Id { get; set; }

        public string Title { get; set; }

        public TimeSpan Duration { get; set; }

        public string FileName { get; set; }

        public string Directory { get; set; }

        public string AbsolutePath { get; set; }

        public string Type { get; set; }
    }
}
