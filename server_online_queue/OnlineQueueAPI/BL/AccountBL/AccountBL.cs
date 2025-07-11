using AutoMapper;
using OnlineQueueAPI.DL;
using OnlineQueueAPI.DL.AccountDL;
using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Models;
using OnlineQueueAPI.Services;
using System.Security.Authentication;
using System.Security.Cryptography;

namespace OnlineQueueAPI.BL
{
    public class AccountBL : BaseBL<User>, IAccountBL
    {
        private readonly IAccountDL _accountDL;
        private readonly IBaseDL<RefreshToken> _refreshTokenDL;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly IOtpBL _otpBL;
        private readonly IUserValidator _userValidator;

        public AccountBL(IHttpContextAccessor httpContextAccessor,
            IBaseDL<User> baseDL,
            IMapper mapper,
            WebSocketService webSocketService,
            IAccountDL accountDL,
            ITokenService tokenService,
            IBaseDL<RefreshToken> refreshTokenDL,
            IOtpBL otpBL,
            IUserValidator userValidator)
            : base(httpContextAccessor, baseDL, mapper, webSocketService)
        {
            _accountDL = accountDL;
            _mapper = mapper;
            _tokenService = tokenService;
            _refreshTokenDL = refreshTokenDL;
            _otpBL = otpBL;
            _userValidator = userValidator;
        }

        public string HashPassword(string password)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[16];
                rng.GetBytes(salt);

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256))
                {
                    byte[] hash = pbkdf2.GetBytes(20);

                    byte[] hashBytes = new byte[36];
                    Array.Copy(salt, 0, hashBytes, 0, 16);
                    Array.Copy(hash, 0, hashBytes, 16, 20);

                    return Convert.ToBase64String(hashBytes);
                }
            }
        }

        public bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            try
            {
                byte[] hashBytes = Convert.FromBase64String(storedPassword);

                byte[] salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);

                using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 100000, HashAlgorithmName.SHA256))
                {
                    byte[] hash = pbkdf2.GetBytes(20);

                    return hashBytes.Skip(16).SequenceEqual(hash);
                }
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public async Task<User> RegisterAsync(AccountDTO.AccountRegisterDTO registerDTO)
        {
            var newUser = _mapper.Map<User>(registerDTO);
            newUser.Id = Guid.NewGuid();
            newUser.Password = HashPassword(registerDTO.Password);

            await AddAsync(newUser, true);

            return newUser;
        }

        public async Task<(string accessToken, string refreshToken)> LoginAsync(AccountDTO.AccountBaseDTO loginDTO)
        {
            var user = await _userValidator.GetByPhoneNumberAsync(loginDTO.PhoneNumber);

            if (user == null || !VerifyPassword(loginDTO.Password, user.Password))
                throw new AuthenticationException("Incorrect phone number or password!");

            var tokens = _tokenService.GenerateToken(user);

            if (string.IsNullOrEmpty(tokens.accessToken) || string.IsNullOrEmpty(tokens.refreshToken))
                throw new InvalidOperationException("Failed to generate token");

            return (tokens.accessToken, tokens.refreshToken);
        }

        public async Task<(string accessToken, string refreshToken)> LoginWithOtpAsync(OtpDTO.OtpVerify otpVerify)
        {
            var user = await _userValidator.GetByPhoneNumberAsync(otpVerify.PhoneNumber);

            if (user == null) throw new AuthenticationException("Incorrect phone number!");

            _otpBL.VerifyOtp(otpVerify.PhoneNumber, otpVerify.Otp);

            var tokens = _tokenService.GenerateToken(user);

            if (string.IsNullOrEmpty(tokens.accessToken) || string.IsNullOrEmpty(tokens.refreshToken))
                throw new InvalidOperationException("Failed to generate token");

            return (tokens.accessToken, tokens.refreshToken);
        }

        public async Task<(string accessToken, string refreshToken)> RefreshToken(AccountDTO.RefreshTokenDTO refreshTokenDTO)
        {
            var storedRefreshToken = _refreshTokenDL
                                        .Query()
                                        .FirstOrDefault(r => r.Token == refreshTokenDTO.RefreshToken && r.Expires > DateTime.UtcNow);

            if (storedRefreshToken == null) throw new ArgumentException("Invalid or expired refresh token");

            var user = await GetByIdAsync(storedRefreshToken.UserId);

            var newToken = _tokenService.GenerateToken(user!);

            return newToken;
        }

        public async Task<User> UpdateUserByIdAsync(Guid id, AccountDTO.AccountUpdateDTO updateDTO)
        {
            var updatedUser = await UpdateAsync(id, updateDTO);
            if (updatedUser == null) throw new InvalidOperationException("Failed to update user");

            return updatedUser;
        }

        public async Task<bool> ChangePasswordAsync(AccountDTO.AccountChangePasswordDTO changePasswordDTO)
        {
            var userId = GetUserId();

            var user = await GetByIdAsync(userId!.Value);

            if (!VerifyPassword(changePasswordDTO.OldPassword, user!.Password))
                throw new InvalidDataException("Old password is incorrect!");

            user.Password = HashPassword(changePasswordDTO.NewPassword);

            var updatedUser = await UpdateAsync(userId.Value, user);

            return updatedUser != null;
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            var user = await _accountDL.GetByUserId(userId);

            return user ?? throw new ArgumentException("Not found user");
        }
    }
}
