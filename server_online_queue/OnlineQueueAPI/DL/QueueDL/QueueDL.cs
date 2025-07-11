using Microsoft.EntityFrameworkCore;
using OnlineQueueAPI.Data;
using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.DL
{
    public class QueueDL : BaseDL<Queue>, IQueueDL
    {
        private readonly OnlineQueueDbContext _dbContext;

        public QueueDL(OnlineQueueDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Queue?> GetQueueWithAppointmentsAsync(Guid queueId)
        {
            return await _dbContext.Queues
                .Include(q => q.Service)
                .Include(q => q.Appointments)
                .FirstOrDefaultAsync(q => q.Id == queueId);
        }

        public async Task<Queue?> GetQueueByService(Guid serviceId, bool priority)
        {
            return await _dbContext.Queues.Where(q => q.ServiceId == serviceId
                                    && q.Type == (priority ? QueueType.Priority : QueueType.Normal))
                                .FirstOrDefaultAsync();
        }
    }
}
