using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.DL
{
    public interface IQueueDL : IBaseDL<Queue>
    {
        Task<Queue?> GetQueueWithAppointmentsAsync(Guid queueId);

        Task<Queue?> GetQueueByService(Guid serviceId, bool priority);
    }
}
