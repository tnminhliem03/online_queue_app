using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineQueueAPI.DL;
using OnlineQueueAPI.Models;
using OnlineQueueAPI.Services;

namespace OnlineQueueAPI.BL
{
    public class UserOrganizationRoleBL : BaseBL<UserOrganizationRole>, IUserOrganizationRoleBL
    {
        private readonly IBaseDL<UserOrganizationRole> _userOrganizationRoleDL;
        private readonly IBaseBL<Organization> _organizationBL;
        private readonly IUserValidator _userValidator;
        private readonly IAccountBL _userBL;

        public UserOrganizationRoleBL(
                IHttpContextAccessor httpContextAccessor,
                IBaseDL<UserOrganizationRole> baseDL,
                IMapper mapper,
                WebSocketService webSocketService,
                IUserValidator userValidator,
                IBaseDL<UserOrganizationRole> userOrganizationRoleDL,
                IBaseBL<Organization> organizationBL,
                IAccountBL userBL
            ) : base(httpContextAccessor, baseDL, mapper, webSocketService)
        {
            _userOrganizationRoleDL = userOrganizationRoleDL;
            _organizationBL = organizationBL;
            _userValidator = userValidator;
            _userBL = userBL;
        }

        public async Task<(
                IEnumerable<Organization> Data,
                int TotalRecords,
                int CurrentPage,
                int TotalPages
            )> GetOrganizationOwnerByUserIdAsync(int? pageNumber, int? pageSize)
        {
            var userId = GetUserId();

            var organizationIds = await _userOrganizationRoleDL
                                                .Query()
                                                .Where(uor => uor.UserId == userId && uor.Role == Role.Manager)
                                                .Select(uor => uor.OrganizationId)
                                                .ToListAsync();

            return await _organizationBL.GetPagedAsync(
                pageNumber,
                pageSize,
                org => organizationIds.Contains(org.Id)
            );
        }

        public async Task<(
                IEnumerable<User> Data,
                int TotalRecords,
                int CurrentPage,
                int TotalPages
            )> GetOrganizationPartnerAsync(int? pageNumber, int? pageSize, Guid organizationId)
        {
            if (organizationId == Guid.Empty) throw new ArgumentNullException("OrganizationId cannot be null");

            var userIds = await _userOrganizationRoleDL
                                .Query()
                                .Where(uor => uor.OrganizationId == organizationId && uor.Role == Role.Manager)
                                .Select(uor => uor.UserId)
                                .ToListAsync();

            return await _userBL.GetPagedAsync(
                pageNumber,
                pageSize,
                u => userIds.Contains(u.Id)
            );
        }

        public async Task<UserOrganizationRole> AddUserOrganizationRoleAsync(Guid userId, Guid organizationId, Role role)
        {
            var userOrgRole = new UserOrganizationRole
            {
                UserId = userId,
                OrganizationId = organizationId,
                Role = role
            };

            var result = await AddAsync(userOrgRole);

            if (result == null) throw new InvalidOperationException("Failed!");

            return result;
        }

        public async Task<UserOrganizationRole> AddStaffOrganizationRoleAsync(string phoneNumber, Guid organizationId, Role role)
        {
            var user = await _userValidator.GetByPhoneNumberAsync(phoneNumber);

            var userOrgRole = new UserOrganizationRole
            {
                UserId = user!.Id,
                OrganizationId = organizationId,
                Role = role
            };

            var result = await AddAsync(userOrgRole, true);

            if (result == null) throw new InvalidOperationException("Failed!");

            return result;
        }

        public async Task<bool> DeletePartnerAsync(Guid userId, Guid organizationId)
        {
            if (organizationId == Guid.Empty) throw new ArgumentNullException("OrganizationId cannot be null");

            var userIds = await _userOrganizationRoleDL
                                .Query()
                               .Where(uor => uor.OrganizationId == organizationId && uor.Role == Role.Manager)
                               .Select(uor => uor.UserId)
                               .ToListAsync();

            if (userIds.Count <= 1) throw new InvalidOperationException("Cannot delete because there is only 1 employee left");

            var partner = await _userOrganizationRoleDL
                                .Query()
                                .FirstOrDefaultAsync(uor => uor.UserId == userId && uor.OrganizationId == organizationId);

            if (partner == null) throw new ArgumentException("User not found");

            var isDeleted = await DeleteAsync(partner.Id);

            return isDeleted;
        }
    }
}
