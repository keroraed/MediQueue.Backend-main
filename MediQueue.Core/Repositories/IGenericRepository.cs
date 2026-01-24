using MediQueue.Core.Specifications;

namespace MediQueue.Core.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T> GetEntityWithSpec(ISpecifications<T> spec);
    Task<IReadOnlyList<T>> ListAsync(ISpecifications<T> spec);
    Task<int> CountAsync(ISpecifications<T> spec);
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}
