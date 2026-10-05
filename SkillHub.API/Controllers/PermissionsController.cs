using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Features.Admin.DTOs;
using SkillHub.Application.Features.Admin.Interfaces;

namespace SkillHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }

        [HttpGet("matrix")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetPermissionMatrix()
        {
            var matrix = await _permissionService.GetPermissionMatrixAsync();
            return Ok(matrix);
        }

        [HttpGet("user-permissions")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserPermissions()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _permissionService.GetUserPermissionsAsync(userId.Value);
            return Ok(result);
        }

        [HttpGet("subadmins")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllSubAdminsWithPermissions()
        {
            var list = await _permissionService.GetAllSubAdminsWithPermissionsAsync();
            return Ok(list);
        }

        [HttpGet("users/{userId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetUserPermissionsDetail(long userId)
        {
            var detail = await _permissionService.GetUserPermissionsDetailAsync(userId);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        [HttpPut("users/{userId}/permissions")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateUserPermissions(long userId, [FromBody] UpdateUserPermissionsRequestDto dto)
        {
            if (userId != dto.UserId)
            {
                dto.UserId = userId;
            }

            try
            {
                await _permissionService.UpdateUserPermissionsAsync(dto);
                return Ok(new { message = "User specific permissions updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("roles")]
        [Authorize(Roles = "SuperAdmin,SubAdmin")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _permissionService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("roles/{roleId}")]
        [Authorize(Roles = "SuperAdmin,SubAdmin")]
        public async Task<IActionResult> GetRoleById(long roleId)
        {
            var role = await _permissionService.GetRoleByIdAsync(roleId);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpPost("roles")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequestDto dto)
        {
            try
            {
                var created = await _permissionService.CreateRoleAsync(dto);
                return Ok(created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("roles/{roleId}/permissions")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateRolePermissions(long roleId, [FromBody] UpdateRolePermissionsRequestDto dto)
        {
            if (roleId != dto.RoleId)
            {
                dto.RoleId = roleId;
            }

            try
            {
                await _permissionService.UpdateRolePermissionsAsync(dto);
                return Ok(new { message = "Role permissions updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
