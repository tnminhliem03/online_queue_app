using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.DL
{
    public interface IOrganizationDL : IBaseDL<Organization>
    {
        Task<Guid?> GetOrganizationIdFromServiceId(Guid serviceId);

        Task<Guid?> GetOrganizationIdFromQueueId(Guid queueId);

        Task<Guid?> GetOrganizationIdFromAppointmentId(Guid appointmentId);

        Task<Role?> GetUserOrganizationRoleWithOrganization(Guid userId, Guid organizationId);

        Task<Role?> GetUserOrganizationRole(Guid userId);

        Task UpdateOrganization(Organization organization);
    }
}
