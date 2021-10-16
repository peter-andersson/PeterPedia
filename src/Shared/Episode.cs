using System;

namespace PeterPedia.Shared
{
    public class Episode
    {
        public Episode()
        {
            Title = string.Empty;
        }

        public int Id { get; set; }

        public string Title { get; set; }

        public int EpisodeNumber { get; set; }

        public DateTime? AirDate { get; set; }

        public bool Watched { get; set; }
    }
}
