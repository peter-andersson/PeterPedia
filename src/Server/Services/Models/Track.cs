using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace PeterPedia.Shared.Services.Models
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

                if (double.TryParse(Duration, NumberStyles.Any, CultureInfo.InvariantCulture, out double tmp))
                {
                    return TimeSpan.FromSeconds(tmp);
                }

                return TimeSpan.FromSeconds(0);
            }
        }
    }
}