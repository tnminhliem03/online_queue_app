using System.Linq.Expressions;

namespace OnlineQueueAPI.DL
{
    public interface IBaseDL<T> where T : class
    {
        IQueryable<T> Query(params Expression<Func<T, object>>[] includes);

        Task<T?> GetById(Guid id, params Expression<Func<T, object>>[] includes);

        Task<T> Add(T entity);

        Task<T> Update(T entity);

        Task<bool> Delete(T entity);
    }
}
