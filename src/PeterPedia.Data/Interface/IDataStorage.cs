using Microsoft.Azure.Cosmos;

namespace PeterPedia.Data.Interface;

public interface IDataStorage<TEntity> where TEntity : IEntity
{
    Task AddAsync(TEntity entity);

    Task DeleteAsync(TEntity entity);

    Task<TEntity?> GetAsync(string id, string partitionKey);
    
    Task<List<TEntity>> QueryAsync(QueryDefinition query);

    Task UpdateAsync(TEntity entity);
}
