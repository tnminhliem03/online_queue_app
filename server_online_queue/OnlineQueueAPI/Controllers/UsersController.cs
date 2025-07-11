using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineQueueAPI.BL;
using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Services;

namespace OnlineQueueAPI.Controllers
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IAccountBL _accountBL;

        public UsersController(IAccountBL accountBL)
        {
            _accountBL = accountBL;
        }

        [Authorize(Policy = "Dynamic")]
        [HttpGet]
        public async Task<IActionResult> GetAllUser(int? pageNumber, int? pageSize)
        {
            try
            {
                var (data, totalRecords, currentPage, totalPages) = await _accountBL.GetPagedAsync(pageNumber ?? 1, pageSize ?? 3);

                string? nextPage = currentPage < totalPages
                        ? $"{Request.Scheme}://{Request.Host}{Url.Action(nameof(GetAllUser),
                            new { pageNumber = currentPage + 1, pageSize })}"
                        : null;

                string? previousPage = currentPage > 1
                        ? $"{Request.Scheme}://{Request.Host}{Url.Action(nameof(GetAllUser),
                            new { pageNumber = currentPage - 1, pageSize })}"
                        : null;

                return Ok(new
                {
                    currentPage,
                    totalRecords,
                    totalPages,
                    nextPage,
                    previousPage,
                    data
                });
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize(Policy = "Owner")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            try
            {
                var user = await _accountBL.GetByIdAsync(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("current-user")]
        public async Task<IActionResult> GetCurrentUser(Guid id)
        {
            try
            {
                var currentUserId = _accountBL.GetUserId();
                var currentUser = await _accountBL.GetUserByIdAsync(currentUserId!.Value);
                return Ok(currentUser);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize(Policy = "Owner")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserById(Guid id, [FromBody] AccountDTO.AccountUpdateDTO updateDTO)
        {
            try
            {
                var updatedUser = await _accountBL.UpdateUserByIdAsync(id, updateDTO);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize(Policy = "Dynamic")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserById(Guid id)
        {
            try
            {
                var deleted = await _accountBL.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }
    }
}
