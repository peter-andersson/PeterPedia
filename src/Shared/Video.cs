using System;

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

        public TimeSpan Duration { get; set; }
    }
}