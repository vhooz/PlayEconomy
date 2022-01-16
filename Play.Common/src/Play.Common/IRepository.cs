using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Play.Common
{
    //We need this interface to be of a generic type T. so we can share code across projects.
    public interface IRepository<T> where T : IEntity
    {
        Task CreateAsync(T entity);
        Task<IReadOnlyCollection<T>> GetAllAsync();

        Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter);

        Task<T> GetAsync(Guid id);

        Task<T> GetAsync(Expression<Func<T, bool>> filter); // lets use a filter to retrive any entity that matches that filter.
        Task RemoveAsync(Guid id);
        Task UpdateAsync(T entity);
    }
}