using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.DTOs
{
    public class OrganizationDTO
    {
        public class OrgUpdateDTO
        {
            public required string Name { get; set; }

            public required string Address { get; set; }

            public required string Hotline { get; set; }

            public string? Email { get; set; }

            public TimeSpan StartTime { get; set; }

            public TimeSpan EndTime { get; set; }
        }

        public class OrgCreateDTO : OrgUpdateDTO
        {
            public Guid FieldId { get; set; }
        }

        public class OrgUpdateStatusDTO
        {
            public OrganizationStatus status { get; set; }
        }
    }
}
