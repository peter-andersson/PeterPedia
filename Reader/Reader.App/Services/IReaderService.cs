namespace Reader.App.Services;

public interface IReaderService
{
    Task<AddResult> AddAsync(NewSubscription subscription);

    Task<bool> Delete(Subscription subscription);

    Task<HistoryArticle[]> GetHistoryAsync();

    Task<UnreadGroup?> GetUnreadGroupAsync(string group);
    
    Task<Subscription?> GetSubscriptionAsync(string id);

    Task<Subscription[]> GetSubscriptionsAsync();

    Task<UnreadGroup[]> UnreadArticlesAsync();

    Task<bool> UpdateSubscriptionAsync(Subscription subscription);
}
