using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.DL
{
    public interface IAppointmentDL : IBaseDL<Appointment>
    {
        Task UpdateAppointment(Appointment appointment);

        Task<bool> GetAppointmentUnfinished(Guid userId);

        Task<List<Appointment>> GetAppointmentsInDayOfOrganization(DateTime date, Guid organizationId);
    }
}
