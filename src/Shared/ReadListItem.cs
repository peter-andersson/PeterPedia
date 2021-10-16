using System;

namespace PeterPedia.Shared
{
    public class ReadListItem
    {
        public ReadListItem()
        {
            Url = string.Empty;
        }

        public int Id { get; set; }

        public string Url { get; set; }

        public DateTime Added { get; set; }
    }
}
