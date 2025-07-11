namespace OnlineQueueAPI.DTOs
{
    public class StatisticsDTO
    {
        public class AppointmentStatisticsDto
        {
            public DateTime SelectedDate { get; set; }

            public Guid OrganizationId { get; set; }

            public int TotalAppointments { get; set; }

            public int TotalPriorityAppointments { get; set; }

            public int TotalNormalAppointments { get; set; }

            public int TotalSkippedAppointments { get; set; }

            public int TotalServicesUsed { get; set; }

            public List<ServiceStatisticsDto> ServiceStatistics { get; set; } = new List<ServiceStatisticsDto>();
        }

        public class ServiceStatisticsDto
        {
            public Guid ServiceId { get; set; }

            public string? ServiceName { get; set; }

            public int TotalAppointments { get; set; }

            public int TotalPriorityAppointments { get; set; }

            public int TotalNormalAppointments { get; set; }

            public int TotalSkippedAppointments { get; set; }
        }
    }
}
