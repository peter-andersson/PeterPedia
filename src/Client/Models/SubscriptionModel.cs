using System.ComponentModel.DataAnnotations;

namespace PeterPedia.Client.Models;

public class SubscriptionModel
{
    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Group { get; set; }

    [Range(5, 60, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
    public int UpdateIntervalMinute { get; set; }
}
