using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OnlineQueueAPI.Models
{
    public class Queue : ICreationInfo
    {
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public Queue()
        {
            Id = Guid.NewGuid();
            _createdAt = DateTime.UtcNow;
            _updatedAt = DateTime.UtcNow;
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public QueueType Type { get; set; }


        public DateTime CreatedAt { get => _createdAt; set => _createdAt = value; }

        public DateTime UpdatedAt { get => _updatedAt; set => _updatedAt = value; }


        [Required]
        public Guid ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public Service? Service { get; set; }

        public List<Appointment>? Appointments { get; set; }
    }

    public enum QueueType
    {
        Normal,
        Priority
    }
}
