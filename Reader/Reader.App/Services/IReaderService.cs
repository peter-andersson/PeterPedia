namespace Reader.App.Services;

public interface IReaderService
{
    Task<AddResult> AddAsync(NewSubscription subscription);
    
    UnreadGroup? GetUnreadGroupAsync(string group);

    Task<UnreadGroup[]> UnreadArticlesAsync();
}
