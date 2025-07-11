using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using OnlineQueueAPI.BL;
using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Services;

namespace OnlineQueueAPI.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountBL _accountBL;

        public AccountsController(IAccountBL accountBL)
        {
            _accountBL = accountBL;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AccountDTO.AccountRegisterDTO registerDTO)
        {
            try
            {
                var newUser = await _accountBL.RegisterAsync(registerDTO);

                return CreatedAtAction(nameof(Register), new { id = newUser.Id }, newUser);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1062)
                    return BadRequest("Phone number or email already exists.");

                throw;
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AccountDTO.AccountBaseDTO loginDTO)
        {
            try
            {
                var (accessToken, refreshToken) = await _accountBL.LoginAsync(loginDTO);
                return Ok(new { accessToken = accessToken, refreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> LoginWithOtp([FromBody] OtpDTO.OtpVerify otpVerifyDTO)
        {
            try
            {
                var (accessToken, refreshToken) = await _accountBL.LoginWithOtpAsync(otpVerifyDTO);
                return Ok(new { accessToken = accessToken, refreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] AccountDTO.RefreshTokenDTO refreshTokenDTO)
        {
            try
            {
                var newAccessToken = await _accountBL.RefreshToken(refreshTokenDTO);
                return Ok(new { newAccessToken = newAccessToken.accessToken, newRefreshToken = newAccessToken.refreshToken });
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] AccountDTO.AccountChangePasswordDTO changePasswordDTO)
        {
            try
            {
                var changePassword = await _accountBL.ChangePasswordAsync(changePasswordDTO);
                return Ok("Change password successfully!");
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }
    }
}
