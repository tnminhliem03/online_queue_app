using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.BL
{
    public interface IAppointmentBL : IBaseBL<Appointment>
    {
        Task<(
                IEnumerable<Appointment> Data,
                int TotalRecords,
                int CurrentPage,
                int TotalPages
            )> GetAppointmentsForServicePagedAsync(int pageNumber, int pageSize, Guid serviceId);

        Task<DateTime> getExpectedTimeAsync(Guid queueId);

        Task<Appointment> AddAppointmentAsync(AppointmentDTO.AppointmentCreateDTO addDTO);

        Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, AppointmentStatus status);

        Task<StatisticsDTO.AppointmentStatisticsDto> GetAppointmentStatisticsAsync(DateTime date, Guid organizationId);
    }
}