using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.BL
{
    public interface INotificationBL : IBaseBL<Notification>
    {
        Task<Notification> AddNotificationAsync(Guid userId, string title, string body, DateTime? scheduledTime);
    }
}
