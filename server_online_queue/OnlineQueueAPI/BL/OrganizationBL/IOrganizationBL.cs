using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.BL
{
    public interface IOrganizationBL : IBaseBL<Organization>
    {
        Task<Organization?> AddOrganization(OrganizationDTO.OrgCreateDTO addOrg);

        Task<bool> UpdateOrganizationStatusAsync(Guid orgId);
    }
}
