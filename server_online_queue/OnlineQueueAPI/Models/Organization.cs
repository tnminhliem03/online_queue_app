using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OnlineQueueAPI.Models
{
    public class Organization : ICreationInfo, IOperatingTime
    {
        private DateTime _createdAt;
        private DateTime _updatedAt;
        private TimeSpan _startTime;
        private TimeSpan _endTime;
        private static int _counter = 1;

        public Organization()
        {
            Id = Guid.NewGuid();
            Status = OrganizationStatus.Opened;
            _createdAt = DateTime.UtcNow;
            _updatedAt = DateTime.UtcNow;
            StartTime = new TimeSpan(7, 0, 0);
            EndTime = new TimeSpan(16, 0, 0);
            Code = GenerateUniqueCode();
        }

        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; }

        [Required, MaxLength(10)]
        public required string Code { get; set; }

        [Required]
        public required string Address { get; set; }

        [Required, MaxLength(15), Phone]
        public required string Hotline { get; set; }

        [MaxLength(150), EmailAddress]
        public string? Email { get; set; }

        public OrganizationStatus Status { get; set; }


        public DateTime CreatedAt { get => _createdAt; set => _createdAt = value; }

        public DateTime UpdatedAt { get => _updatedAt; set => _updatedAt = value; }

        public TimeSpan StartTime { get => _startTime; set => _startTime = value; }

        public TimeSpan EndTime { get => _endTime; set => _endTime = value; }


        [Required]
        public Guid FieldId { get; set; }

        [ForeignKey("FieldId")]
        public Field? Field { get; set; }

        public List<Service>? Services { get; set; }

        [JsonIgnore]
        public ICollection<UserOrganizationRole>? UserOrganizationRoles { get; set; }

        private string GenerateUniqueCode()
        {
            string prefix = GenerateRandomPrefix();
            string suffix = _counter.ToString("D3");
            _counter++;

            return prefix + suffix;
        }

        private string GenerateRandomPrefix()
        {
            Random random = new Random();

            string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string prefix = "";

            for (int i = 0; i < 3; i++)
                prefix += letters[random.Next(0, letters.Length)];

            return prefix;
        }
    }

    public enum OrganizationStatus
    {
        Opened,
        Closed
    }
}
