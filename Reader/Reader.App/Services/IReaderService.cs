namespace Reader.App.Services;

public interface IReaderService
{
    Task<AddResult> AddAsync(NewSubscription subscription);

    Task<bool> Delete(Subscription subscription);

    Task<HistoryArticle[]> GetHistoryAsync();
    
    UnreadGroup? GetUnreadGroup(string group);
    
    Subscription? GetSubscription(string id);

    Task<Subscription[]> GetSubscriptionsAsync();

    Task<UnreadGroup[]> UnreadArticlesAsync();

    Task<bool> UpdateSubscriptionAsync(Subscription subscription);
}
