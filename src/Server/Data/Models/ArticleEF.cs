using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Server.Data.Models
{
    [Table("article")]
    public class ArticleEF
    {
        private string? _title;

        private string? _content;

        public int Id { get; set; }

        [MaxLength(100)]
        public string Title
        {
            get
            {
                return _title ?? string.Empty;
            }
            set
            {
                if (value is null)
                {
                    value = string.Empty;
                }

                _title = value.Length > 100 ? value.Substring(0, 100) : value;
            }
        }

        public string Url { get; set; } = null!;

        public DateTime PublishDate { get; set; }

        [MaxLength(2000)]
        public string Content
        {
            get
            {
                return _content ?? string.Empty;
            }
            set
            {
                if (value is null)
                {
                    value = string.Empty;
                }

                _content = value.Length > 2000 ? value.Substring(0, 2000) : value;
            }
        }

        public DateTime? ReadDate { get; set; }

        public int SubscriptionId { get; set; }

        public SubscriptionEF Subscription { get; set; } = null!;
    }
}
