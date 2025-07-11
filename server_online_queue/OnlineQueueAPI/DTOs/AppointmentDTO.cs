using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.DTOs
{
    public class AppointmentDTO
    {
        public class AppointmentUpdateDTO
        {
            public required bool Priority { get; set; }

            public required AppointmentStatus Status { get; set; }
        }

        public class AppointmentCreateDTO
        {
            public required bool Priority { get; set; }

            public required Guid ServiceId { get; set; }
        }
    }
}
