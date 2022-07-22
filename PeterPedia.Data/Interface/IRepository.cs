using Microsoft.Azure.Cosmos;

namespace PeterPedia.Data.Interface;

public interface IRepository
{
    Task AddAsync<TEntity>(TEntity entity) where TEntity : IEntity;

    Task DeleteAsync<TEntity>(TEntity entity) where TEntity : IEntity;

    Task<TEntity?> GetAsync<TEntity>(string id) where TEntity : IEntity;
    
    Task<List<TEntity>> QueryAsync<TEntity>(QueryDefinition query) where TEntity : IEntity;

    Task UpdateAsync<TEntity>(TEntity entity) where TEntity : IEntity;

    Task AddAsync<TEntity>(string containerId, TEntity entity) where TEntity : IEntity;

    Task DeleteAsync<TEntity>(string containerId, TEntity entity) where TEntity : IEntity;

    Task<TEntity?> GetAsync<TEntity>(string containerId, string id) where TEntity : IEntity;

    Task<List<TEntity>> QueryAsync<TEntity>(string containerId, QueryDefinition query) where TEntity : IEntity;

    Task UpdateAsync<TEntity>(string containerId, TEntity entity) where TEntity : IEntity;
}
