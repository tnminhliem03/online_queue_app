using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OnlineQueueAPI.Models
{
    public class Appointment : ICreationInfo
    {
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public Appointment()
        {
            Id = Guid.NewGuid();
            Status = AppointmentStatus.Pending;
            Priority = false;
            _createdAt = DateTime.UtcNow;
            _updatedAt = DateTime.UtcNow;
        }

        [Key]
        public Guid Id { get; set; }

        public AppointmentStatus Status { get; set; }

        public bool Priority { get; set; }

        public int QueueNumber { get; set; }

        [Required]
        public DateTime ExpectedDateTime { get; set; }


        public DateTime CreatedAt { get => _createdAt; set => _createdAt = value; }

        public DateTime UpdatedAt { get => _updatedAt; set => _updatedAt = value; }


        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public Guid QueueId { get; set; }

        [ForeignKey("QueueId")]
        public Queue? Queue { get; set; }
    }

    public enum AppointmentStatus
    {
        Pending,
        CheckedIn,
        Turn,
        InProgress,
        Completed,
        Skipped,
    }
}
