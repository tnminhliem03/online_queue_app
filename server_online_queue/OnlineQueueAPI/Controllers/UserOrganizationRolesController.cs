using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineQueueAPI.BL;
using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Models;
using OnlineQueueAPI.Services;

namespace OnlineQueueAPI.Controllers
{
    [Authorize]
    [Route("api/roles")]
    [ApiController]
    public class UserOrganizationRolesController : ControllerBase
    {
        private readonly IUserOrganizationRoleBL _userOrganizationRoleBL;

        public UserOrganizationRolesController(IUserOrganizationRoleBL userOrganizationRoleBL)
        {
            _userOrganizationRoleBL = userOrganizationRoleBL;
        }

        [Authorize]
        [HttpGet]
        [Route("owner")]
        public async Task<IActionResult> GetOwner(int? pageNumber, int? pageSize)
        {
            try
            {
                var result = await _userOrganizationRoleBL.GetOrganizationOwnerByUserIdAsync(pageNumber, pageSize);

                string? nextPage = result.CurrentPage < result.TotalPages
                        ? $"{Request.Scheme}://{Request.Host}{Url.Action(nameof(GetOwner),
                            new { pageNumber = result.CurrentPage + 1, pageSize })}"
                        : null;

                string? previousPage = result.CurrentPage > 1
                        ? $"{Request.Scheme}://{Request.Host}{Url.Action(nameof(GetOwner),
                            new { pageNumber = result.CurrentPage - 1, pageSize })}"
                        : null;

                return Ok(new
                {
                    data = result.Data,
                    totalRecords = result.TotalRecords,
                    currentPage = result.CurrentPage,
                    totalPages = result.TotalPages,
                    nextPage,
                    previousPage,
                });
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize(Policy = "Dynamic")]
        [HttpGet]
        [Route("partner")]
        public async Task<IActionResult> GetPartner(int? pageNumber, int? pageSize, [FromQuery] Guid organizationId)
        {
            try
            {
                var result = await _userOrganizationRoleBL.GetOrganizationPartnerAsync(pageNumber, pageSize, organizationId);

                string? nextPage = result.CurrentPage < result.TotalPages
                        ? $"{Request.Scheme}://{Request.Host}{Url.Action(nameof(GetOwner),
                            new { pageNumber = result.CurrentPage + 1, pageSize })}"
                        : null;

                string? previousPage = result.CurrentPage > 1
                        ? $"{Request.Scheme}://{Request.Host}{Url.Action(nameof(GetOwner),
                            new { pageNumber = result.CurrentPage - 1, pageSize })}"
                        : null;

                return Ok(new
                {
                    data = result.Data,
                    totalRecords = result.TotalRecords,
                    currentPage = result.CurrentPage,
                    totalPages = result.TotalPages,
                    nextPage,
                    previousPage,
                });
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddCustomerToOrganization([FromBody] UserOrganizationRoleDTO.RoleCreate addDto)
        {
            try
            {
                var userId = _userOrganizationRoleBL.GetUserId();

                await _userOrganizationRoleBL.AddUserOrganizationRoleAsync(userId!.Value, addDto.OrganizationId, Role.Customer);

                return StatusCode(StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize(Policy = "Dynamic")]
        [HttpPost("add-staff")]
        public async Task<IActionResult> AddStaffToOrganization([FromBody] UserOrganizationRoleDTO.StaffCreate addDto)
        {
            try
            {
                await _userOrganizationRoleBL.AddStaffOrganizationRoleAsync(addDto.PhoneNumber, addDto.OrganizationId, Role.Manager);

                return StatusCode(StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeletePartner([FromBody] UserOrganizationRoleDTO.StaffDelete deleteDTO)
        {
            try
            {
                var deleted = await _userOrganizationRoleBL.DeletePartnerAsync(deleteDTO.UserId, deleteDTO.OrganizationId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }
    }

}