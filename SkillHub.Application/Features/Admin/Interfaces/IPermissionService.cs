using System.Collections.Generic;
using System.Threading.Tasks;
using SkillHub.Application.Features.Admin.DTOs;

namespace SkillHub.Application.Features.Admin.Interfaces
{
    public interface IPermissionService
    {
        Task<PermissionMatrixDto> GetPermissionMatrixAsync();
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<RoleDetailDto?> GetRoleByIdAsync(long roleId);
        Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto dto);
        Task UpdateRolePermissionsAsync(UpdateRolePermissionsRequestDto dto);
        Task<UserPermissionsResponseDto> GetUserPermissionsAsync(long userId);
        
        // Granular SubAdmin & User Management
        Task<List<SubAdminUserDetailDto>> GetAllSubAdminsWithPermissionsAsync();
        Task<SubAdminUserDetailDto?> GetUserPermissionsDetailAsync(long userId);
        Task UpdateUserPermissionsAsync(UpdateUserPermissionsRequestDto dto);
    }
}
