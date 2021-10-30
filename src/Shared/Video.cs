using System;
using System.Text.Json.Serialization;

namespace PeterPedia.Shared
{
    public class Video
    {
        public Video()
        {
            Title = string.Empty;
            Url = string.Empty;
        }

        public int Id { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public string Type { get; set; }

        [JsonConverter(typeof(JsonTimeSpanConverter))]
        public TimeSpan Duration { get; set; }
    }
}