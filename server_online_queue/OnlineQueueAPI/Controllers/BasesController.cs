using Microsoft.AspNetCore.Mvc;
using OnlineQueueAPI.BL;
using OnlineQueueAPI.Services;

namespace OnlineQueueAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasesController<T, TCreateDto, TUpdateDto> : ControllerBase where T : class
    {
        private readonly IBaseBL<T> _baseBL;

        public BasesController(IBaseBL<T> baseBL)
        {
            _baseBL = baseBL;
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetAll(int? pageNumber, int? pageSize)
        {
            try
            {
                var (data, totalRecords, currentPage, totalPages) = await _baseBL.GetPagedAsync(pageNumber ?? 1, pageSize ?? 3);

                string? nextPage = currentPage < totalPages
                        ? $"{Request.Scheme}://{Request.Host}{Url.Action(nameof(GetAll),
                            new { pageNumber = currentPage + 1, pageSize })}"
                        : null;

                string? previousPage = currentPage > 1
                        ? $"{Request.Scheme}://{Request.Host}{Url.Action(nameof(GetAll),
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

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetByID(Guid id)
        {
            try
            {
                var entity = await _baseBL.GetByIdAsync(id);
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> Add([FromBody] TCreateDto addDto)
        {
            try
            {
                var addEntity = await _baseBL.AddAsync(addDto, true);

                var entityIdProp = typeof(T).GetProperty("Id");
                if (entityIdProp == null) return BadRequest("Entity does not have an 'Id' property.");

                var entityId = entityIdProp.GetValue(addEntity, null);
                if (entityId == null) return BadRequest("Failed to retrieve entity ID.");

                return CreatedAtAction(nameof(GetByID), new { id = entityId }, addEntity);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Update(Guid id, [FromBody] TUpdateDto updateDto)
        {
            try
            {
                var updatedEntity = await _baseBL.UpdateAsync(id, updateDto);
                return Ok(updatedEntity);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _baseBL.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }
    }
}
