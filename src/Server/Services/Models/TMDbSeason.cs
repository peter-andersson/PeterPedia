using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PeterPedia.Server.Services.Models
{
    public class TMDbSeason
    {
        public TMDbSeason()
        {
            Episodes = new List<TMDbEpisode>();
        }

        [JsonPropertyName("season_number")]
        public int SeasonNumber { get; set; }

        [JsonPropertyName("episodes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Deserialised from JSON.")]
        public List<TMDbEpisode> Episodes { get; set; }
    }
}
