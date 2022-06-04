using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Data.Models;

[Table("episode")]
public class EpisodeEF
{
    public EpisodeEF() => Title = string.Empty;

    public int Id { get; set; }

    public string Title { get; set; }

    public int EpisodeNumber { get; set; }

    public DateTime? AirDate { get; set; }

    public bool Watched { get; set; }

    public int SeasonId { get; set; }

    public SeasonEF Season { get; set; } = null!;
}
