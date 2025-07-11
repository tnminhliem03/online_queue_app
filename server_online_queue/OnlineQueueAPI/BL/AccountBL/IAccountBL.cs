using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.BL
{
    public interface IAccountBL : IBaseBL<User>
    {
        string HashPassword(string password);

        bool VerifyPassword(string enteredPassword, string storedPassword);

        Task<User> RegisterAsync(AccountDTO.AccountRegisterDTO registerDTO);

        Task<(string accessToken, string refreshToken)> LoginAsync(AccountDTO.AccountBaseDTO loginDTO);

        Task<(string accessToken, string refreshToken)> LoginWithOtpAsync(OtpDTO.OtpVerify otpVerify);

        Task<(string accessToken, string refreshToken)> RefreshToken(AccountDTO.RefreshTokenDTO refreshTokenDTO);

        Task<User> UpdateUserByIdAsync(Guid id, AccountDTO.AccountUpdateDTO updateDTO);

        Task<bool> ChangePasswordAsync(AccountDTO.AccountChangePasswordDTO changePasswordDTO);

        Task<User?> GetUserByIdAsync(Guid userId);
    }
}
