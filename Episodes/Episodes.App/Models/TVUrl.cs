using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Episodes.App.Models;

public class TVUrl
{
    [Required]
    [RegularExpression(@"^https://www.themoviedb.org/tv/(\d+)", ErrorMessage = "Needs to be a valid url from themoviedb.org")]
    public string? Url { get; set; }

    public int? Id
    {
        get
        {
            var regex = new Regex(@"^https://www.themoviedb.org/tv/(\d+)");

            if (regex.IsMatch(Url ?? string.Empty))
            {
                Match? matches = regex.Match(Url ?? string.Empty);

                if (int.TryParse(matches.Groups[1].Value, out var movieId))
                {
                    return movieId;
                }
            }

            return null;
        }
    }
}
