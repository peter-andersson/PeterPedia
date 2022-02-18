using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Server.Data.Models
{
    [Table("subscription")]
    public class SubscriptionEF
    {
        private string? _title;

        public SubscriptionEF()
        {
            Articles = new List<ArticleEF>();
        }

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

                _title = value.Length > 100 ? value[..100] : value;
            }
        }

        public string Url { get; set; } = null!;

        [MaxLength(32)]
        public byte[] Hash { get; set; } = null!;

        [Range(5, 60, ErrorMessage = "Value must be between 5 and 60")]
        public int UpdateIntervalMinute { get; set; }

        public string? Group { get; set; }

        public DateTime LastUpdate { get; set; }

        public IList<ArticleEF> Articles { get; private set; }
    }
}
