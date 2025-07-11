namespace OnlineQueueAPI.DTOs
{
    public class NotificationDTO
    {
        public required string Title { get; set; }

        public required string Body { get; set; }

        public required Guid UserId { get; set; }

        public DateTime? ScheduledTime { get; set; }
    }
}
