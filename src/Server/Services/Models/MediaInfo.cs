using System;

namespace PeterPedia.Shared.Services.Models
{
    public class MediaInfo
    {
        public MediaInfo()
        {
            Title = string.Empty;
            FileExtension = string.Empty;
            Duration = TimeSpan.FromSeconds(0);
        }

        public string Title { get; set; }

        public string FileExtension { get; set; }

        public TimeSpan Duration { get; set; }
    }
}