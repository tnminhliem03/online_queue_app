using OnlineQueueAPI.DL.AccountDL;
using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.Services
{
    public class UserValidator : IUserValidator
    {
        private readonly IAccountDL _accountDL;

        public UserValidator(IAccountDL accountDL)
        {
            _accountDL = accountDL;
        }

        public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
        {
            if (!phoneNumber.All(char.IsDigit) || phoneNumber.Length != 9)
                throw new InvalidDataException("Invalid Phone Number.");

            return await _accountDL.GetByPhoneNumber(phoneNumber);
        }
    }
}