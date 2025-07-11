using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineQueueAPI.BL;
using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Models;
using OnlineQueueAPI.Services;

namespace OnlineQueueAPI.Controllers
{
    [Authorize]
    [Route("api/notifications")]
    [ApiController]
    public class NotificationsController : BasesController<Notification, NotificationDTO, NotificationDTO>
    {
        private readonly INotificationBL _notificationBL;
        public NotificationsController(IBaseBL<Notification> baseBL, INotificationBL notificationBL) : base(baseBL)
        {
            _notificationBL = notificationBL;
        }

        [Authorize(Policy = "Admin")]
        [HttpGet("all")]
        public override async Task<IActionResult> GetAll(int? pageNumber, int? pageSize)
        {
            try
            {
                var (data, totalRecords, currentPage, totalPages) = await _notificationBL.GetPagedAsync(
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
        public async Task<IActionResult> GetAllForUser(int? pageNumber, int? pageSize, [FromQuery] Guid? userId)
        {
            try
            {
                if (!userId.HasValue) return BadRequest("userId is required.");

                Expression<Func<Notification, bool>>? filter = userId.HasValue
                    ? n => n.UserId == userId.Value
                    : null;

                var (data, totalRecords, currentPage, totalPages) = await _notificationBL.GetPagedAsync(
                    pageNumber ?? 1, pageSize ?? 3, filter);

                return Ok(new
                {
                    currentPage,
                    pageSize = pageSize ?? 3,
                    totalRecords,
                    totalPages,
                    nextPage = currentPage < totalPages ? Url.Action(nameof(GetAllForUser), new { pageNumber = currentPage + 1, pageSize, userId }) : null,
                    previousPage = currentPage > 1 ? Url.Action(nameof(GetAllForUser), new { pageNumber = currentPage - 1, pageSize, userId }) : null,
                    data
                });
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize]
        public override Task<IActionResult> GetByID(Guid id)
        {
            return base.GetByID(id);
        }

        [Authorize]
        public override async Task<IActionResult> Add([FromBody] NotificationDTO addDto)
        {
            try
            {
                var addEntity = await _notificationBL.AddNotificationAsync(addDto.UserId, addDto.Title, addDto.Body, addDto.ScheduledTime);

                var entityId = addEntity.Id;

                return CreatedAtAction(nameof(GetByID), new { id = entityId }, addEntity);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize(Policy = "Admin")]
        public override Task<IActionResult> Update(Guid id, [FromBody] NotificationDTO updateDto)
        {
            return base.Update(id, updateDto);
        }

        [Authorize(Policy = "Admin")]
        public override Task<IActionResult> Delete(Guid id)
        {
            return base.Delete(id);
        }
    }
}
