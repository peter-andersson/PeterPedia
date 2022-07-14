namespace PeterPedia.Data.Interface;

public interface IEntity
{
    string Id { get; }

    string PartitionKey { get; }
}
