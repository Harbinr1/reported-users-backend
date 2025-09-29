using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportedUsersSystem.DTOs;
using ReportedUsersSystem.Services;

namespace ReportedUsersSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportedUsersController : ControllerBase
    {
        private readonly ReportedUserService _reportedUserService;

        public ReportedUsersController(ReportedUserService reportedUserService)
        {
            _reportedUserService = reportedUserService;
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] ReportedUserStatusUpdateDto dto)
        {
            try
            {
                var updated = await _reportedUserService.UpdateReportedUserStatusAsAdmin(id, dto.Status);
                return Ok(updated);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("drafts/admin")]
        public async Task<IActionResult> GetDraftsForAdmin()
        {
            try
            {
                var drafts = await _reportedUserService.GetDraftReportedUsersForAdmin();
                return Ok(drafts);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var reportedUsers = await _reportedUserService.GetAllReportedUsers();
                return Ok(reportedUsers);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var reportedUser = await _reportedUserService.GetReportedUserById(id);
                return Ok(reportedUser);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReportedUserCreateDto reportedUserDto)
        {
            try
            {
                var createdUser = await _reportedUserService.CreateReportedUser(reportedUserDto);
                return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, ReportedUserUpdateDto reportedUserDto)
        {
            try
            {
                var updatedUser = await _reportedUserService.UpdateReportedUser(id, reportedUserDto);
                return Ok(updatedUser);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _reportedUserService.DeleteReportedUser(id);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string NameOrIdNumber)
        {
            try
            {
                var results = await _reportedUserService.SearchReportedUsers(NameOrIdNumber);
                return Ok(results);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}