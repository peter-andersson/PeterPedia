namespace Reader.App.Services;

public interface IReaderService
{
    Task<UnreadGroup[]> UnreadArticles();

    UnreadGroup? GetUnreadGroup(string group);
}
