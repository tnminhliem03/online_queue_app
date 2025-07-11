using System.Linq.Expressions;

namespace OnlineQueueAPI.BL
{
    public interface IBaseBL<T> where T : class
    {
        Guid? GetUserId();

        Task<(
                IEnumerable<T> Data,
                int TotalRecords,
                int CurrentPage,
                int TotalPages
            )> GetPagedAsync(
                    int? pageNumber,
                    int? pageSize,
                    Expression<Func<T, bool>>? filter = null,
                    params Expression<Func<T, object>>[] includes
                );

        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);

        Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes);

        Task<T?> AddAsync<TDto>(TDto createDto, bool isSend = false);

        Task<T?> UpdateAsync<TDto>(Guid id, TDto updateDto);

        Task<bool> DeleteAsync(Guid id);
    }
}