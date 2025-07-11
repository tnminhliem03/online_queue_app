using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineQueueAPI.BL;
using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.Controllers
{
    [Authorize]
    [Route("api/fields")]
    [ApiController]
    public class FieldsController : BasesController<Field, FieldDTO, FieldDTO>
    {
        public FieldsController(IBaseBL<Field> baseBL) : base(baseBL) { }

        [Authorize]
        public override Task<IActionResult> GetAll(int? pageNumber, int? pageSize)
        {
            return base.GetAll(pageNumber ?? 1, pageSize ?? 3);
        }

        [Authorize]
        public override Task<IActionResult> GetByID(Guid id)
        {
            return base.GetByID(id);
        }

        [Authorize(Policy = "Dynamic")]
        public override Task<IActionResult> Add([FromBody] FieldDTO addDto)
        {
            return base.Add(addDto);
        }

        [Authorize(Policy = "Dynamic")]
        public override Task<IActionResult> Update(Guid id, [FromBody] FieldDTO updateDto)
        {
            return base.Update(id, updateDto);
        }

        [Authorize(Policy = "Dynamic")]
        public override Task<IActionResult> Delete(Guid id)
        {
            return base.Delete(id);
        }
    }
}
