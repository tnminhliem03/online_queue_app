using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using OnlineQueueAPI.BL;
using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Models;
using OnlineQueueAPI.Services;

namespace OnlineQueueAPI.Controllers
{
    [Authorize]
    [Route("api/organizations")]
    [ApiController]
    public class OrganizationsController : BasesController<Organization, OrganizationDTO.OrgCreateDTO,
        OrganizationDTO.OrgUpdateDTO>
    {
        private readonly IOrganizationBL _organizationBL;

        public OrganizationsController(IBaseBL<Organization> baseBL, IOrganizationBL organizationBL) : base(baseBL)
        {
            _organizationBL = organizationBL;
        }

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

        [Authorize]
        public override async Task<IActionResult> Add([FromBody] OrganizationDTO.OrgCreateDTO addDto)
        {
            try
            {
                var newOrg = await _organizationBL.AddOrganization(addDto);

                return CreatedAtAction(nameof(GetByID), new { id = newOrg!.Id }, newOrg);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1062)
                    return BadRequest("Code, Hotline or Email already exists.");

                throw;
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize(Policy = "Dynamic")]
        public override Task<IActionResult> Update(Guid id, [FromBody] OrganizationDTO.OrgUpdateDTO updateDto)
        {
            return base.Update(id, updateDto);
        }

        [Authorize(Policy = "Dynamic")]
        [HttpPut("{id}/update-status")]
        public async Task<IActionResult> UpdateStatus(Guid id)
        {
            try
            {
                var updatedEntity = await _organizationBL.UpdateOrganizationStatusAsync(id);
                return Ok("Status update successful!");
            }
            catch (Exception ex)
            {
                return ExceptionHelper.HandleException(ex);
            }
        }

        [Authorize(Policy = "Dynamic")]
        public override Task<IActionResult> Delete(Guid id)
        {
            return base.Delete(id);
        }
    }
}
