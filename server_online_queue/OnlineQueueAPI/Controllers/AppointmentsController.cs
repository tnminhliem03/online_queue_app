using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineQueueAPI.BL;
using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Models;
using OnlineQueueAPI.Services;

namespace OnlineQueueAPI.Controllers
{
    [Authorize]
    [Route("api/appointments")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentBL _appointmentBL;

        public AppointmentsController(IAppointmentBL appointmentBL)
        {
            _appointmentBL = appointmentBL;
        }

        [Authorize(Policy = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll(int? pageNumber, int? pageSize)
        {
            try
            {
                var (data, totalRecords, currentPage, totalPages) = await _appointmentBL.GetPagedAsync(
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
        public async Task<IActionResult> GetAllForService(int? pageNumber, int? pageSize, [FromQuery] Guid serviceId)
        {
            try
            {
                var (data, totalRecords, currentPage, totalPages) = await _appointmentBL.GetAppointmentsForServicePagedAsync(
                    pageNumber ?? 1, pageSize ?? 3, serviceId);

                return Ok(new
                {
                    currentPage,
                    pageSize = pageSize ?? 3,
                    totalRecords,
                    totalPages,
                    nextPage = currentPage < totalPages ? Url.Action(nameof(GetAllForService), new { pageNumber = currentPage + 1, pageSize, serviceId }) : null,
                    previousPage = currentPage > 1 ? Url.Action(nameof(GetAllForService), new { pageNumber = currentPage - 1, pageSize, serviceId }) : null,
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
                var entity = await _appointmentBL.GetByIdAsync(id);
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize]
        [HttpGet("expected-time/{queueId}")]
        public async Task<IActionResult> GetExpectedTime(Guid queueId)
        {
            try
            {
                var expectedTime = await _appointmentBL.getExpectedTimeAsync(queueId);
                return Ok(expectedTime);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize(Policy = "Dynamic")]
        [HttpGet("statistics")]
        public async Task<IActionResult> GetAppointmentStatistics(
            [FromQuery] DateTime date,
            [FromQuery] Guid organizationId)
        {
            try
            {
                var statistics = await _appointmentBL.GetAppointmentStatisticsAsync(date, organizationId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AppointmentDTO.AppointmentCreateDTO addDTO)
        {
            try
            {
                var addEntity = await _appointmentBL.AddAppointmentAsync(addDTO);

                var entityId = addEntity.Id;
                if (entityId == Guid.Empty) return BadRequest("Failed to retrieve entity ID.");

                return CreatedAtAction(nameof(GetById), new { id = entityId }, addEntity);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize]
        [HttpPut("{id}/check-in")]
        public async Task<IActionResult> CheckInAppointment(Guid id)
        {
            try
            {
                await _appointmentBL.UpdateAppointmentStatusAsync(id, AppointmentStatus.CheckedIn);

                return Ok("Status update successful!");
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize(Policy = "Dynamic")]
        [HttpPut("{id}/turn")]
        public async Task<IActionResult> TurnAppointment(Guid id)
        {
            try
            {
                await _appointmentBL.UpdateAppointmentStatusAsync(id, AppointmentStatus.Turn);

                return Ok("Status update successful!");
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize(Policy = "Dynamic")]
        [HttpPut("{id}/in-progress")]
        public async Task<IActionResult> InProgressAppointment(Guid id)
        {
            try
            {
                await _appointmentBL.UpdateAppointmentStatusAsync(id, AppointmentStatus.InProgress);

                return Ok("Status update successful!");
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize(Policy = "Dynamic")]
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteAppointment(Guid id)
        {
            try
            {
                await _appointmentBL.UpdateAppointmentStatusAsync(id, AppointmentStatus.Completed);

                return Ok("Status update successful!");
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize(Policy = "Dynamic")]
        [HttpPut("{id}/skip")]
        public async Task<IActionResult> SkipAppointment(Guid id)
        {
            try
            {
                await _appointmentBL.UpdateAppointmentStatusAsync(id, AppointmentStatus.Skipped);

                return Ok("Status update successful!");
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }
    }
}
