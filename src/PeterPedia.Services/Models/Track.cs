using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace PeterPedia.Services.Models
{
    public class Track
    {
        [JsonPropertyName("@type")]
        public string? Type { get; set; }

        [JsonPropertyName("Duration")]
        public string? Duration { get; set; }

        public TimeSpan DurationTimeSpan
        {
            get
            {
                if (Duration is null)
                {
                    return TimeSpan.FromSeconds(0);
                }

                // 
                return double.TryParse(Duration, NumberStyles.Any, CultureInfo.InvariantCulture, out var tmp)
                    ? TimeSpan.FromSeconds(tmp)
                    : TimeSpan.FromSeconds(0);
            }
        }
    }
}
