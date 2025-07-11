using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.DTOs
{
    public class UserOrganizationRoleDTO
    {
        public class RoleCreate
        {
            public required Guid OrganizationId { get; set; }
        }

        public class StaffCreate : RoleCreate
        {
            public required string PhoneNumber { get; set; }
        }

        public class StaffDelete : RoleCreate
        {
            public required Guid UserId { get; set; }
        }
    }
}
