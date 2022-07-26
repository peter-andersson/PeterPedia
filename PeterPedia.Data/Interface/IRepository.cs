using Microsoft.Azure.Cosmos;
using PeterPedia.Data.Models;

namespace PeterPedia.Data.Interface;

public interface IRepository
{
    Task AddAsync<TEntity>(TEntity entity) where TEntity : BaseEntity;

    Task DeleteAsync<TEntity>(TEntity entity) where TEntity : BaseEntity;

    Task<TEntity?> GetAsync<TEntity>(string id) where TEntity : BaseEntity;
    
    Task<List<TEntity>> QueryAsync<TEntity>(QueryDefinition query) where TEntity : BaseEntity;

    Task UpdateAsync<TEntity>(TEntity entity) where TEntity : BaseEntity;

    Task AddAsync<TEntity>(string containerId, TEntity entity) where TEntity : BaseEntity;

    Task DeleteAsync<TEntity>(string containerId, TEntity entity) where TEntity : BaseEntity;

    Task<TEntity?> GetAsync<TEntity>(string containerId, string id) where TEntity : BaseEntity;

    Task<List<TEntity>> QueryAsync<TEntity>(string containerId, QueryDefinition query) where TEntity : BaseEntity;

    Task UpdateAsync<TEntity>(string containerId, TEntity entity) where TEntity : BaseEntity;
}
