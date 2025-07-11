using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.DL.AccountDL
{
    public interface IAccountDL : IBaseDL<User>
    {
        Task<User?> GetByPhoneNumber(string phoneNumber);

        Task<User?> GetByUserId(Guid userId);
    }
}
