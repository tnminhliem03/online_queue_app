using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineQueueAPI.DL;
using OnlineQueueAPI.Models;
using OnlineQueueAPI.Services;
using System.Linq.Expressions;
using System.Security.Authentication;
using System.Security.Claims;

namespace OnlineQueueAPI.BL
{
    public class BaseBL<T> : IBaseBL<T> where T : class
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBaseDL<T> _baseDL;
        private readonly IMapper _mapper;
        private readonly WebSocketService _webSocketService;
        private string EntityType => typeof(T).Name;

        public BaseBL(
                IHttpContextAccessor httpContextAccessor,
                IBaseDL<T> baseDL,
                IMapper mapper,
                WebSocketService webSocketService
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _baseDL = baseDL;
            _mapper = mapper;
            _webSocketService = webSocketService;
        }

        public Guid? GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity?.IsAuthenticated == true)
                throw new AuthenticationException("User is not logged in");

            var userIdString = user?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out
                var userId))
                throw new AuthenticationException("Invalid Token or missing UserId");

            return userId;
        }

        public async Task<(
                IEnumerable<T> Data,
                int TotalRecords,
                int CurrentPage,
                int TotalPages
            )> GetPagedAsync(
                    int? pageNumber,
                    int? pageSize,
                    Expression<Func<T, bool>>? filter = null,
                    params Expression<Func<T, object>>[] includes
                )
        {
            int defaultPageSize = 3;
            int defaultPageNumber = 1;

            int currentPage = pageNumber ?? defaultPageNumber;
            int currentSize = pageSize ?? defaultPageSize;

            if (currentPage < 1 || currentSize <= 0)
                throw new InvalidDataException("Invalid Current Page or Current Size");

            var query = _baseDL.Query(includes);

            if (filter != null)
                query = query.Where(filter);

            int totalRecords = await query.CountAsync();

            var paginatedData = await query
                                    .Skip((currentPage - 1) * currentSize)
                                    .Take(currentSize)
                                    .ToListAsync();

            int totalPages = (int)Math.Ceiling((double)totalRecords / currentSize);

            if (currentPage > totalPages && totalRecords > 0)
                throw new InvalidOperationException("Number of pages exceeds total number of pages.");

            return (paginatedData, totalRecords, currentPage, totalPages);
        }

        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            var result = await _baseDL
                                .Query(includes)
                                .ToListAsync();
                                
            if (!result.Any()) throw new ArgumentException("Not found data");

            return result;
        }

        public async Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes)
        {
            var entity = await _baseDL.GetById(id, includes);

            return entity ??
              throw new ArgumentException("Not found data");
        }

        public async Task<T?> AddAsync<TDto>(TDto createDto, bool isSend = false)
        {
            if (createDto == null) throw new ArgumentNullException(nameof(createDto), "Input data cannot be empty");

            var entity = _mapper.Map<T>(createDto);
            if (entity == null)
                throw new AutoMapperMappingException($"Cannot map from {typeof(TDto).Name} to {typeof(T).Name}.");

            var result = await _baseDL.Add(entity);
            if (result == null) throw new InvalidOperationException("Failed to add data");

            if (isSend) await _webSocketService.SendUpdateToClients(EntityType);

            return result;
        }

        public async Task<T?> UpdateAsync<TDto>(Guid id, TDto updateDto)
        {
            if (updateDto == null) throw new ArgumentNullException(nameof(updateDto), "Input data cannot be empty");

            var existingEntity = await GetByIdAsync(id);

            try
            {
                _mapper.Map(updateDto, existingEntity);
            }
            catch (Exception ex)
            {
                throw new AutoMapperMappingException($"Error mapping data: {ex.Message}");
            }

            if (existingEntity is ICreationInfo trackableEntity)
                trackableEntity.UpdatedAt = DateTime.UtcNow;

            var result = await _baseDL.Update(existingEntity!);

            if (result == null)
                throw new InvalidOperationException("Failed to update data");

            await _webSocketService.SendUpdateToClients(EntityType);

            return result;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);

            var isDeleted = await _baseDL.Delete(entity!);

            if (!isDeleted)
                throw new InvalidOperationException($"Failed to delete data");

            await _webSocketService.SendUpdateToClients(EntityType);

            return isDeleted;
        }
    }
}