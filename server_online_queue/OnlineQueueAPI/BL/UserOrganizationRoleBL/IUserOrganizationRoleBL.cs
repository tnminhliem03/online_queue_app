using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.BL
{
    public interface IUserOrganizationRoleBL : IBaseBL<UserOrganizationRole>
    {
        Task<(
                IEnumerable<Organization> Data,
                int TotalRecords,
                int CurrentPage,
                int TotalPages
            )> GetOrganizationOwnerByUserIdAsync(int? pageNumber, int? pageSize);

        Task<(
                IEnumerable<User> Data,
                int TotalRecords,
                int CurrentPage,
                int TotalPages
            )> GetOrganizationPartnerAsync(int? pageNumber, int? pageSize, Guid organizationId);

        Task<UserOrganizationRole> AddUserOrganizationRoleAsync(Guid userId, Guid organizationId, Role role);

        Task<UserOrganizationRole> AddStaffOrganizationRoleAsync(String phoneNumber, Guid organizationId, Role role);

        Task<bool> DeletePartnerAsync(Guid userId, Guid organizationId);
    }
}
