namespace OnlineQueueAPI.Models
{
    public interface IOperatingTime
    {
        TimeSpan StartTime { get; set; }
        TimeSpan EndTime { get; set; }
    }
}
