using AutoMapper;
using OnlineQueueAPI.DL;
using OnlineQueueAPI.Models;
using OnlineQueueAPI.Services;

namespace OnlineQueueAPI.BL
{
    public class NotificationBL : BaseBL<Notification>, INotificationBL
    {
        public NotificationBL(IHttpContextAccessor httpContextAccessor,
            IBaseDL<Notification> baseDL,
            IMapper mapper,
            WebSocketService webSocketService)
            : base(httpContextAccessor, baseDL, mapper, webSocketService) { }

        public async Task<Notification> AddNotificationAsync(Guid userId, string title, string body, DateTime? scheduledTime)
        {
            var notification = new Notification
            {
                Title = title,
                Body = body,
                UserId = userId,
                CreatedAt = scheduledTime?.ToUniversalTime() ?? DateTime.Now,
                UpdatedAt = scheduledTime?.ToUniversalTime() ?? DateTime.Now,
            };

            var created = await AddAsync(notification, true);

            return created!;
        }
    }
}