using System.ComponentModel.DataAnnotations;

namespace PeterPedia.Client.Models
{
    public class ShowModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Title must be specified.")]
        public string? Title { get; set; }

        public bool Refresh { get; set; }
    }
}
