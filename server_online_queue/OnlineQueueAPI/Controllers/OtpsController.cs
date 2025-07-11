using Microsoft.AspNetCore.Mvc;
using OnlineQueueAPI.BL;
using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Services;

namespace OnlineQueueAPI.Controllers
{
    [ApiController]
    [Route("api/otp")]
    public class OtpController : ControllerBase
    {
        private readonly IOtpBL _otpBL;

        public OtpController(IOtpBL otpBL)
        {
            _otpBL = otpBL;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateOtp([FromBody] OtpDTO.OtpRequest request)
        {
            try
            {
                var (otp, expiresAt) = await _otpBL.GenerateOtpAsync(request.PhoneNumber);
                _otpBL.StoreOtp(request.PhoneNumber, otp, expiresAt);

                return Ok(new { Otp = otp, ExpiresAt = expiresAt });
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }
    }
}