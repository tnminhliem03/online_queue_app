using Microsoft.EntityFrameworkCore;
using OnlineQueueAPI.Data;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace OnlineQueueAPI.DL
{
    public class BaseDL<T> : IBaseDL<T> where T : class
    {
        private readonly OnlineQueueDbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public BaseDL(OnlineQueueDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        public IQueryable<T> Query(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            var allIncludes = GetNavigationPaths(typeof(T));

            foreach (var include in allIncludes)
            {
                query = query.Include(include);
            }

            foreach(var include in includes)
            {
                query = query.Include(include);
            }

            return query;
        }

        private List<string> GetNavigationPaths(Type entityType, string parentPath = "", HashSet<Type> visitedEntities = null!)
        {
            var includes = new List<string>();

            if (visitedEntities == null)
                visitedEntities = new HashSet<Type>();

            if (visitedEntities.Contains(entityType))
                return includes;

            visitedEntities.Add(entityType);

            var entity = _dbContext.Model.FindEntityType(entityType);
            if (entity == null) return includes;

            foreach(var nav in entity.GetNavigations())
            {
                if (string.IsNullOrEmpty(nav.Name)) continue;

                var fullPath = string.IsNullOrEmpty(parentPath) ? nav.Name : $"{parentPath}.{nav.Name}";
                includes.Add(fullPath);

                if (nav.TargetEntityType?.ClrType != null)
                    includes.AddRange(GetNavigationPaths(nav.TargetEntityType.ClrType, fullPath, visitedEntities));
            }

            return includes;
        }

        public async Task<T?> GetById(Guid id, params Expression<Func<T, object>>[] includes)
        {
            var query = Query(includes);

            var keyProperty = typeof(T).GetProperties()
                .FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Any());

            return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, keyProperty!.Name) == id);
        }

        public async Task<T> Add(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<T> Update(T entity)
        {
            _dbSet.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> Delete(T entity)
        {
            _dbSet.Remove(entity);
            await _dbContext.SaveChangesAsync();

            return true;
        }        
    }
}
