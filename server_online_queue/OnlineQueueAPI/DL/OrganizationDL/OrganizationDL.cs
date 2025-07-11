using Microsoft.EntityFrameworkCore;
using OnlineQueueAPI.Data;
using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.DL
{
    public class OrganizationDL : BaseDL<Organization>, IOrganizationDL
    {
        private readonly OnlineQueueDbContext _dbContext;

        public OrganizationDL(OnlineQueueDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Role?> GetUserOrganizationRoleWithOrganization(Guid userId, Guid organizationId)
        {
            return await _dbContext.UserOrganizationsRoles
                .Where(uor => uor.UserId == userId && uor.OrganizationId == organizationId)
                .Select(uor => (Role?)uor.Role)
                .FirstOrDefaultAsync();
        }

        public async Task<Role?> GetUserOrganizationRole(Guid userId)
        {
            return await _dbContext.UserOrganizationsRoles
                .Where(uor => uor.UserId == userId)
                .Select(uor => (Role?)uor.Role)
                .FirstOrDefaultAsync();
        }

        public async Task<Guid?> GetOrganizationIdFromServiceId(Guid serviceId)
        {
            var service = await _dbContext.Services.FindAsync(serviceId);
            return service?.OrganizationId;
        }

        public async Task<Guid?> GetOrganizationIdFromQueueId(Guid queueId)
        {
            var queue = await _dbContext.Queues.FindAsync(queueId);
            return queue == null ? null : await GetOrganizationIdFromServiceId(queue.ServiceId);
        }

        public async Task<Guid?> GetOrganizationIdFromAppointmentId(Guid appointmentId)
        {
            var appointment = await _dbContext.Appointments.FindAsync(appointmentId);
            return appointment == null ? null : await GetOrganizationIdFromQueueId(appointment.QueueId);
        }

        public async Task UpdateOrganization(Organization organization)
        {
            _dbContext.Organizations.Update(organization);
            await _dbContext.SaveChangesAsync();
        }
    }
}
