namespace PeterPedia.Services;

public interface IReaderManager
{
    Task<bool> AddSubscriptionAsync(string url);

    Task<bool> DeleteArticleAsync(int id);

    Task<bool> DeleteSubscriptionAsync(int id);

    Task<List<Subscription>> GetSubscriptionsAsync();

    Task<List<Article>> GetHistoryAsync();

    Task<Subscription?> GetSubscriptionAsync(int id);

    Task<List<UnreadArticle>> GetUnreadAsync();

    Task<bool> UpdateSubscriptionAsync(Subscription subscription);
}
