using AutoMapper;
using OnlineQueueAPI.DL;
using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Models;
using OnlineQueueAPI.Services;

namespace OnlineQueueAPI.BL
{
    public class OrganizationBL : BaseBL<Organization>, IOrganizationBL
    {
        private readonly IUserOrganizationRoleBL _userOrgRoleBL;
        private readonly IOrganizationDL _organizationDL;
        private readonly WebSocketService _webSocketService;

        public OrganizationBL(
                IHttpContextAccessor httpContextAccessor,
                IBaseDL<Organization> baseDL,
                IMapper mapper,
                WebSocketService webSocketService,
                IUserOrganizationRoleBL userOrgRoleBL,
                IOrganizationDL organizationDL
            ) : base(httpContextAccessor, baseDL, mapper, webSocketService)
        {
            _userOrgRoleBL = userOrgRoleBL;
            _organizationDL = organizationDL;
            _webSocketService = webSocketService;
        }

        public async Task<Organization?> AddOrganization(OrganizationDTO.OrgCreateDTO addOrg)
        {
            var organization = await AddAsync(addOrg, true);

            var organizationId = organization!.Id;
            if (organizationId == Guid.Empty) throw new InvalidDataException("Invalid Organization Id");

            var userId = GetUserId();
            await _userOrgRoleBL.AddUserOrganizationRoleAsync(userId!.Value, organizationId, Role.Manager);

            return organization;
        }

        public async Task<bool> UpdateOrganizationStatusAsync(Guid orgId)
        {
            var organization = await GetByIdAsync(orgId);

            organization!.Status = (organization.Status == OrganizationStatus.Opened) ?
               OrganizationStatus.Closed :
               OrganizationStatus.Opened;

            await _organizationDL.UpdateOrganization(organization);

            await _webSocketService.SendUpdateToClients("Organization");

            return true;
        }
    }
}