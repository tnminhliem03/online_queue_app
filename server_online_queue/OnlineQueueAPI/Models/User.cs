using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OnlineQueueAPI.Models
{
    public class User : ICreationInfo
    {
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public User()
        {
            Id = Guid.NewGuid();
            Role = UserRole.Customer;
            _createdAt = DateTime.UtcNow;
            _updatedAt = DateTime.UtcNow;
        }

        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; }

        [MaxLength(150), EmailAddress]
        public string? Email { get; set; }

        [Required, MaxLength(15), Phone]
        public required string PhoneNumber { get; set; }

        [Required, MaxLength(255), JsonIgnore]
        public required string Password { get; set; }

        [Required]
        public UserRole Role { get; set; }


        public DateTime CreatedAt { get => _createdAt; set => _createdAt = value; }
        public DateTime UpdatedAt { get => _updatedAt; set => _updatedAt = value; }

        public List<Appointment>? Appointments { get; set; }

        public ICollection<UserOrganizationRole>? UserOrganizationRoles { get; set; }

        [JsonIgnore]
        public ICollection<RefreshToken>? RefreshTokens { get; set; }

        [JsonIgnore]
        public ICollection<Notification>? Notifications { get; set; }
    }

    public enum UserRole
    {
        Admin,
        Customer
    }
}
