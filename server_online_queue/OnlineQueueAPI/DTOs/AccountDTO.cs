namespace OnlineQueueAPI.DTOs
{
    public class AccountDTO
    {
        public class AccountBaseDTO
        {
            public required string PhoneNumber { get; set; }

            public required string Password { get; set; }
        }

        public class AccountRegisterDTO : AccountBaseDTO
        {
            public required string Name { get; set; }
        }

        public class AccountUpdateDTO
        {
            public required string Name { get; set; }

            public string? Email { get; set; }

            public required string PhoneNumber { get; set; }
        }

        public class AccountChangePasswordDTO
        {
            public required string OldPassword { get; set; }

            public required string NewPassword { get; set; }
        }

        public class RefreshTokenDTO
        {
            public required string RefreshToken { get; set; }
        }

        public class RequestOtpDTO
        {
            public required string PhoneNumber { get; set; }
        }

        public class VerifyOtpDto
        {
            public required string PhoneNumber { get; set; }
            public required string OtpCode { get; set; }
        }
    }
}
