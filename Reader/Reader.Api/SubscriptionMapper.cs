namespace Reader.Api;

public static class SubscriptionMapper
{
    public static Subscription ConvertToDTO(this SubscriptionEntity entity) =>
        new()
        {
            Id = entity.Id,
            Title = entity.Title,
            Url = entity.Url,
            UpdateIntervalMinute = entity.UpdateIntervalMinute,
            Group = entity.Group,
            LastUpdated = entity.LastUpdated,
            UpdateAt = entity.UpdateAt,
            NextUpdate = entity.NextUpdate,
        };
}
