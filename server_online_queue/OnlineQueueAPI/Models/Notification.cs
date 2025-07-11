using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace OnlineQueueAPI.Models
{
    public class Notification : ICreationInfo
    {
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public Notification()
        {
            Id = Guid.NewGuid();
            _createdAt = DateTime.UtcNow;
            _updatedAt = DateTime.UtcNow;
        }

        [Key]
        public Guid Id { get; set; }
        
        [Required, MaxLength(150)]
        public required string Title { get; set; }

        [Required, MaxLength(255)]
        public required string Body { get; set; }


        public DateTime CreatedAt { get => _createdAt; set => _createdAt = value; }

        public DateTime UpdatedAt { get => _updatedAt; set => _updatedAt = value; }


        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}