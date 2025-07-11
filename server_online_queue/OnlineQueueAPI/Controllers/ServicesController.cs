using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineQueueAPI.BL;
using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Models;
using OnlineQueueAPI.Services;
using System.Linq.Expressions;

namespace OnlineQueueAPI.Controllers
{
    [Authorize]
    [Route("api/services")]
    [ApiController]
    public class ServicesController : ControllerBase

    {
        private readonly IServiceBL _serviceBL;

        public ServicesController(IServiceBL serviceBL)
        {
            _serviceBL = serviceBL;
        }

        [Authorize(Policy = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll(int? pageNumber, int? pageSize)
        {
            try
            {
                var (data, totalRecords, currentPage, totalPages) = await _serviceBL.GetPagedAsync(
                    pageNumber ?? 1, pageSize ?? 3);

                return Ok(new
                {
                    currentPage,
                    pageSize = pageSize ?? 3,
                    totalRecords,
                    totalPages,
                    nextPage = currentPage < totalPages ? Url.Action(nameof(GetAll), new { pageNumber = currentPage + 1, pageSize }) : null,
                    previousPage = currentPage > 1 ? Url.Action(nameof(GetAll), new { pageNumber = currentPage - 1, pageSize }) : null,
                    data
                });
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllForOrganization(int? pageNumber, int? pageSize, [FromQuery] Guid? organizationId)
        {
            try
            {
                if (!organizationId.HasValue) return BadRequest("organizationId is required.");

                Expression<Func<Service, bool>>? filter = organizationId.HasValue
                    ? s => s.OrganizationId == organizationId.Value
                    : null;

                var (data, totalRecords, currentPage, totalPages) = await _serviceBL.GetPagedAsync(
                    pageNumber ?? 1, pageSize ?? 3, filter);

                return Ok(new
                {
                    currentPage,
                    pageSize = pageSize ?? 3,
                    totalRecords,
                    totalPages,
                    nextPage = currentPage < totalPages ? Url.Action(nameof(GetAllForOrganization), new { pageNumber = currentPage + 1, pageSize, organizationId }) : null,
                    previousPage = currentPage > 1 ? Url.Action(nameof(GetAllForOrganization), new { pageNumber = currentPage - 1, pageSize, organizationId }) : null,
                    data
                });
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var entity = await _serviceBL.GetByIdAsync(id);
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize(Policy = "Dynamic")]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ServiceDTO.ServiceCreateDTO addDto)
        {
            try
            {
                var addEntity = await _serviceBL.AddServiceAsync(addDto);

                var entityId = addEntity.Id;
                if (entityId == Guid.Empty) return BadRequest("Failed to retrieve entity ID.");

                return CreatedAtAction(nameof(GetById), new { id = entityId, organizationId = addDto.OrganizationId }, addEntity);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize(Policy = "Dynamic")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id,

            [FromBody] ServiceDTO.ServiceUpdateDTO updateDto)
        {
            try
            {
                var updatedEntity = await _serviceBL.UpdateServiceAsync(id, updateDto);
                return Ok(updatedEntity);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize(Policy = "Dynamic")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _serviceBL.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }
    }
}
