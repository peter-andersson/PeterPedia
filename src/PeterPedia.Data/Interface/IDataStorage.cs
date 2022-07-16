using Microsoft.Azure.Cosmos;

namespace PeterPedia.Data.Interface;

public interface IDataStorage<TEntity> where TEntity : IEntity
{
    public Container Container { get; }

    Task AddAsync(TEntity entity);

    Task DeleteAsync(TEntity entity);

    Task<TEntity?> GetAsync(string id);
    
    Task<List<TEntity>> QueryAsync(QueryDefinition query);

    Task UpdateAsync(TEntity entity);    
}
