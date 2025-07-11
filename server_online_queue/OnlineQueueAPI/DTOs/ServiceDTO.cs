
using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.DTOs
{
    public class ServiceDTO
    {
        public class ServiceUpdateDTO
        {
            public required string Name { get; set; }

            public string? Description { get; set; }

            public int AverageDuration { get; set; }

            public TimeSpan StartTime { get; set; }

            public TimeSpan EndTime { get; set; }
        }

        public class ServiceCreateDTO : ServiceUpdateDTO
        {
            public Guid OrganizationId { get; set; }
        }
    }
}
