using System.ComponentModel.DataAnnotations;

namespace OnlineQueueAPI.Models
{
    public class Field : ICreationInfo
    {
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public Field()
        {
            Id = Guid.NewGuid();
            _createdAt = DateTime.UtcNow;
            _updatedAt = DateTime.UtcNow;
        }

        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; }

        public string? Description { get; set; }


        public DateTime CreatedAt { get => _createdAt; set => _createdAt = value; }
        public DateTime UpdatedAt { get => _updatedAt; set => _updatedAt = value; }


        public List<Organization>? Organizations { get; set; }
    }
}
