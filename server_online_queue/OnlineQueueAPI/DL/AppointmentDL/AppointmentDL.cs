using Microsoft.EntityFrameworkCore;
using OnlineQueueAPI.Data;
using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.DL
{
    public class AppointmentDL : BaseDL<Appointment>, IAppointmentDL
    {
        private readonly OnlineQueueDbContext _dbContext;

        public AppointmentDL(OnlineQueueDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task UpdateAppointment(Appointment appointment)
        {
            _dbContext.Appointments.Update(appointment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> GetAppointmentUnfinished(Guid userId)
        {
            return await _dbContext.Appointments.AnyAsync(a => a.UserId == userId
                                        && a.Status != AppointmentStatus.Completed
                                        && a.Status != AppointmentStatus.Skipped);
        }

        public async Task<List<Appointment>> GetAppointmentsInDayOfOrganization(DateTime date, Guid organizationId)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddSeconds(-1);

            return await _dbContext.Appointments
                .Where(a => a.Queue != null &&
                    a.Queue.Service != null &&
                    a.Queue.Service.OrganizationId == organizationId &&
                    a.ExpectedDateTime >= startOfDay &&
                    a.ExpectedDateTime <= endOfDay)
                .Include(a => a.Queue)
                    .ThenInclude(q => q!.Service)
                .ToListAsync();
        }
    }
}
