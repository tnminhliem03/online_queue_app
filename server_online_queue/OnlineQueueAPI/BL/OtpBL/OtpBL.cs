using System.Collections.Concurrent;
using System.Security.Authentication;
using OnlineQueueAPI.Services;

namespace OnlineQueueAPI.BL
{
    public class OtpBL : IOtpBL
    {
        private IUserValidator _userValidator;
        private static ConcurrentDictionary<string, (string otp, DateTime expiresAt)> otpStore = new();

        public OtpBL(IUserValidator userValidator)
        {
            _userValidator = userValidator;
        }

        public async Task<(string otp, DateTime expiresAt)> GenerateOtpAsync(string phoneNumber)
        {
            if (await _userValidator.GetByPhoneNumberAsync(phoneNumber) == null)
                throw new ArgumentException("Invalid phone number");

            var otp = new Random().Next(100000, 999999).ToString();
            var expiresAt = DateTime.UtcNow.AddMinutes(3);

            StoreOtp(phoneNumber, otp, expiresAt);

            return (otp, expiresAt);
        }

        public void VerifyOtp(string phoneNumber, string otp)
        {
            if (!otpStore.TryGetValue(phoneNumber, out var otpEntry))
                throw new ArgumentException("OTP not found for this phone number");

            if (DateTime.UtcNow > otpEntry.expiresAt)
            {
                otpStore.TryRemove(phoneNumber, out _);
                throw new InvalidOperationException("OTP has expired.");
            }

            if (otpEntry.otp != otp) throw new AuthenticationException("Invalid OTP.");

            otpStore.TryRemove(phoneNumber, out _);
        }

        public void StoreOtp(string phoneNumber, string otp, DateTime expiresAt)
        {
            otpStore[phoneNumber] = (otp, expiresAt);
        }
    }
}