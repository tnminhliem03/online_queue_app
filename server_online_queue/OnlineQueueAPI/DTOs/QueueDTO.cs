using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.DTOs
{
    public class QueueDTO
    {
        public class QueueUpdateDTO
        {
            public required QueueType Type { get; set; }
        }

        public class QueueCreateDTO : QueueUpdateDTO
        {
            public required Guid ServiceId { get; set; }
        }
    }
}
