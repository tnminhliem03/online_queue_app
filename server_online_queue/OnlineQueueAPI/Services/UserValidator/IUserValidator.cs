using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.Services
{
    public interface IUserValidator
    {
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    }
}