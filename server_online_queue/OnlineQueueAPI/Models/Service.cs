using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OnlineQueueAPI.Models
{
    public class Service : ICreationInfo, IOperatingTime
    {
        private DateTime _createdAt;
        private DateTime _updatedAt;
        private TimeSpan _startTime;
        private TimeSpan _endTime;

        public Service()
        {
            Id = Guid.NewGuid();
            Status = ServiceStatus.Opened;
            _createdAt = DateTime.UtcNow;
            _updatedAt = DateTime.UtcNow;
            StartTime = new TimeSpan(7, 0, 0);
            EndTime = new TimeSpan(16, 0, 0);
        }

        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; }

        public string? Description { get; set; }

        public int AverageDuration { get; set; }

        public ServiceStatus Status { get; set; }


        public DateTime CreatedAt { get => _createdAt; set => _createdAt = value; }

        public DateTime UpdatedAt { get => _updatedAt; set => _updatedAt = value; }

        public TimeSpan StartTime { get => _startTime; set => _startTime = value; }

        public TimeSpan EndTime { get => _endTime; set => _endTime = value; }


        [Required]
        public Guid OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public Organization? Organization { get; set; }

        public List<Queue>? Queues { get; set; }
    }

    public enum ServiceStatus
    {
        Opened,
        Closed,
        Busy
    }
}
