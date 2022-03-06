using System.ComponentModel.DataAnnotations;

namespace PeterPedia.Client.Models;

public class AuthorModel
{

        [Required(AllowEmptyStrings = false, ErrorMessage = "Name must be specified.")]
        [StringLength(100, ErrorMessage = "Name is too long.")]
        public string? Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Date of birth must be specified.")]
        public DateTime? DateOfBirth { get; set; }
}
