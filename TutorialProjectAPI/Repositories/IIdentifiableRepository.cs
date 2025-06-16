using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TutorialProjectAPI.Repositories
{
    /// <summary>
    /// Generic repository interface for any entity that has a Guid Id.
    /// </summary>
    public interface IIdentifiableRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id, bool track = false);   // ← new ‘track’ flag
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<IEnumerable<T>> GetAllAsync();
        Task SaveAsync();
    }

}
