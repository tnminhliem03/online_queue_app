using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OnlineQueueAPI.Models
{
    public class UserOrganizationRole : ICreationInfo
    {
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public UserOrganizationRole()
        {
            Id = Guid.NewGuid();
            _createdAt = DateTime.Now;
            _updatedAt = DateTime.Now;
            Role = Role.Customer;
        }

        [Key]
        public Guid Id { get; set; }

        public Role Role { get; set; }


        public DateTime CreatedAt { get => _createdAt; set => _createdAt = value; }

        public DateTime UpdatedAt { get => _updatedAt; set => _updatedAt = value; }


        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public Guid OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public Organization? Organization { get; set; }
    }

    public enum Role
    {
        Manager,
        Customer
    }
}
