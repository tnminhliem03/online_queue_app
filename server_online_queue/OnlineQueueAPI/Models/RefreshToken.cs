using System.ComponentModel.DataAnnotations;

namespace OnlineQueueAPI.Models
{
    public class RefreshToken
    {
        [Key]
        public required string Token { get; set; }

        public Guid UserId { get; set; }

        public DateTime Expires { get; set; }

        public User? User { get; set; }
    }
}