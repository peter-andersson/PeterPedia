using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PeterPedia.Server.Services.Models
{
    public class TMDbShow
    {
        public TMDbShow()
        {
            Seasons = new List<TMDbSeason>();
            Status = string.Empty;
            Title = string.Empty;
            ETag = string.Empty;
        }

        /// <summary>
        /// Checks if the tv show has a valid id > 0.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return Id > 0;
            }
        }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("original_name")]
        public string Title { get; set; }

        [JsonPropertyName("last_air_date")]
        public DateTime? LastAirDate { get; set; }

        public string ETag { get; set; }

        [JsonPropertyName("seasons")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Deserialised from JSON.")]
        public List<TMDbSeason> Seasons { get; set; }
    }
}
