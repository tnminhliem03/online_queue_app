namespace OnlineQueueAPI.DTOs
{
    public class OtpDTO
    {
        public class OtpRequest
        {
            public required string PhoneNumber { get; set; }
        }

        public class OtpVerify
        {
            public required string PhoneNumber { get; set; }

            public required string Otp { get; set; }
        }
    }
}