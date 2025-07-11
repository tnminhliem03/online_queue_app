namespace OnlineQueueAPI.BL
{
    public interface IOtpBL
    {
        Task<(string otp, DateTime expiresAt)> GenerateOtpAsync(string phoneNumber);

        void VerifyOtp(string phoneNumber, string otp);

        void StoreOtp(string phoneNumber, string otp, DateTime expiresAt);
    }
}