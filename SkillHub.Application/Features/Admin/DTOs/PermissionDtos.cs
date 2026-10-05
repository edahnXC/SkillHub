using System;
using System.Collections.Generic;

namespace SkillHub.Application.Features.Admin.DTOs
{
    public class PermissionDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
    }

    public class PermissionCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public List<PermissionDto> Permissions { get; set; } = new();
    }

    public class RoleDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PermissionsCount { get; set; }
        public int UsersCount { get; set; }
        public bool IsSystemRole { get; set; }
    }

    public class RoleDetailDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<long> PermissionIds { get; set; } = new();
        public List<string> PermissionNames { get; set; } = new();
    }

    public class UpdateRolePermissionsRequestDto
    {
        public long RoleId { get; set; }
        public List<long> PermissionIds { get; set; } = new();
    }

    public class CreateRoleRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<long> PermissionIds { get; set; } = new();
    }

    public class PermissionMatrixDto
    {
        public List<RoleDto> Roles { get; set; } = new();
        public List<PermissionCategoryDto> Categories { get; set; } = new();
        public Dictionary<long, List<long>> RolePermissions { get; set; } = new(); // RoleId -> List<PermissionId>
    }

    public class UserPermissionsResponseDto
    {
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }
}
