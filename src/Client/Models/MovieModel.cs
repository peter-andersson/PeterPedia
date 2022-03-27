using System;
using System.ComponentModel.DataAnnotations;

namespace PeterPedia.Client.Models
{
    public class MovieModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Title must be specified.")]
        public string? Title { get; set; }

        public DateTime? WatchedDate { get; set; }
    }
}
