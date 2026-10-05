using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Features.Admin.DTOs;
using SkillHub.Application.Features.Admin.Interfaces;
using SkillHub.Domain.Entities.Identity;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Services.Admin
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static string GetCategoryForPermission(string permissionName)
        {
            if (permissionName.StartsWith("Tabs.")) return "Navigation & UI Tabs";
            if (permissionName.StartsWith("Courses.")) return "Course Management";
            if (permissionName.StartsWith("Users.")) return "User Management";
            if (permissionName.StartsWith("Categories.") || permissionName.StartsWith("Tags.")) return "Taxonomy & Categories";
            if (permissionName.StartsWith("Roles.") || permissionName.StartsWith("Admins.")) return "Roles & Permissions";
            if (permissionName.StartsWith("Instructors.")) return "Instructor Management";
            if (permissionName.StartsWith("Financials.") || permissionName.StartsWith("Reports.")) return "Financials & Reports";
            if (permissionName.StartsWith("Quizzes.")) return "Assessments & Quizzes";
            if (permissionName.StartsWith("Reviews.")) return "Reviews & Ratings";
            if (permissionName.StartsWith("Support.")) return "Support & Helpdesk";
            return "General";
        }

        public async Task<PermissionMatrixDto> GetPermissionMatrixAsync()
        {
            var roles = await _context.Roles
                .Include(r => r.UserRoles)
                .Include(r => r.RolePermissions)
                .OrderBy(r => r.Id)
                .AsNoTracking()
                .ToListAsync();

            var permissions = await _context.Permissions
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync();

            var roleDtos = roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                PermissionsCount = r.RolePermissions.Count,
                UsersCount = r.UserRoles.Count,
                IsSystemRole = r.Name is "SuperAdmin" or "SubAdmin" or "Instructor" or "Student"
            }).ToList();

            var categoryGroups = permissions
                .GroupBy(p => GetCategoryForPermission(p.Name))
                .OrderBy(g => g.Key)
                .Select(g => new PermissionCategoryDto
                {
                    CategoryName = g.Key,
                    Permissions = g.Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Category = g.Key
                    }).OrderBy(p => p.Name).ToList()
                }).ToList();

            var rolePermissionsDict = new Dictionary<long, List<long>>();
            foreach (var role in roles)
            {
                rolePermissionsDict[role.Id] = role.RolePermissions.Select(rp => rp.PermissionId).ToList();
            }

            return new PermissionMatrixDto
            {
                Roles = roleDtos,
                Categories = categoryGroups,
                RolePermissions = rolePermissionsDict
            };
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _context.Roles
                .Include(r => r.UserRoles)
                .Include(r => r.RolePermissions)
                .OrderBy(r => r.Id)
                .AsNoTracking()
                .ToListAsync();

            return roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                PermissionsCount = r.RolePermissions.Count,
                UsersCount = r.UserRoles.Count,
                IsSystemRole = r.Name is "SuperAdmin" or "SubAdmin" or "Instructor" or "Student"
            }).ToList();
        }

        public async Task<RoleDetailDto?> GetRoleByIdAsync(long roleId)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null) return null;

            return new RoleDetailDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                PermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList(),
                PermissionNames = role.RolePermissions.Select(rp => rp.Permission.Name).ToList()
            };
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Role name cannot be empty.");
            }

            var trimmedName = dto.Name.Trim();
            var exists = await _context.Roles.AnyAsync(r => r.Name == trimmedName);
            if (exists)
            {
                throw new ArgumentException($"Role '{trimmedName}' already exists.");
            }

            var role = new Role
            {
                Name = trimmedName,
                Description = dto.Description?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            if (dto.PermissionIds != null && dto.PermissionIds.Count > 0)
            {
                var rolePerms = dto.PermissionIds.Select(pid => new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = pid,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.RolePermissions.AddRangeAsync(rolePerms);
                await _context.SaveChangesAsync();
            }

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                PermissionsCount = dto.PermissionIds?.Count ?? 0,
                UsersCount = 0,
                IsSystemRole = false
            };
        }

        public async Task UpdateRolePermissionsAsync(UpdateRolePermissionsRequestDto dto)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == dto.RoleId);

            if (role == null)
            {
                throw new KeyNotFoundException("Role not found.");
            }

            _context.RolePermissions.RemoveRange(role.RolePermissions);

            if (dto.PermissionIds != null && dto.PermissionIds.Count > 0)
            {
                var distinctIds = dto.PermissionIds.Distinct().ToList();
                var validPermissionIds = await _context.Permissions
                    .Where(p => distinctIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                var newRolePerms = validPermissionIds.Select(pid => new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = pid,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.RolePermissions.AddRangeAsync(newRolePerms);
            }

            role.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<UserPermissionsResponseDto> GetUserPermissionsAsync(long userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .Include(u => u.UserPermissions)
                    .ThenInclude(up => up.Permission)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();

            // 1. Role-derived permissions
            var rolePermissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name);

            // 2. User-specific custom permissions
            var customPermissions = user.UserPermissions
                .Where(up => up.IsGranted)
                .Select(up => up.Permission.Name);

            // Union distinct
            var allPermissions = rolePermissions
                .Concat(customPermissions)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            return new UserPermissionsResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Roles = roles,
                Permissions = allPermissions
            };
        }

        public async Task<List<SubAdminUserDetailDto>> GetAllSubAdminsWithPermissionsAsync()
        {
            var subAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "SubAdmin");
            if (subAdminRole == null) return new List<SubAdminUserDetailDto>();

            var subAdminUsers = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .Include(u => u.UserPermissions)
                    .ThenInclude(up => up.Permission)
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == subAdminRole.Id))
                .OrderBy(u => u.FirstName)
                .AsNoTracking()
                .ToListAsync();

            var result = new List<SubAdminUserDetailDto>();

            foreach (var user in subAdminUsers)
            {
                var configuredPermIds = user.UserPermissions
                    .Where(up => up.IsGranted)
                    .Select(up => up.PermissionId)
                    .ToList();

                // If user has no specific override entries, fallback to the role's permissions
                if (configuredPermIds.Count == 0)
                {
                    configuredPermIds = user.UserRoles
                        .SelectMany(ur => ur.Role.RolePermissions)
                        .Select(rp => rp.PermissionId)
                        .Distinct()
                        .ToList();
                }

                var effectiveNames = user.UserPermissions
                    .Where(up => up.IsGranted)
                    .Select(up => up.Permission.Name)
                    .Concat(user.UserRoles.SelectMany(ur => ur.Role.RolePermissions).Select(rp => rp.Permission.Name))
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();

                result.Add(new SubAdminUserDetailDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    ConfiguredPermissionIds = configuredPermIds,
                    EffectivePermissionNames = effectiveNames
                });
            }

            return result;
        }

        public async Task<SubAdminUserDetailDto?> GetUserPermissionsDetailAsync(long userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .Include(u => u.UserPermissions)
                    .ThenInclude(up => up.Permission)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            var configuredPermIds = user.UserPermissions
                .Where(up => up.IsGranted)
                .Select(up => up.PermissionId)
                .ToList();

            if (configuredPermIds.Count == 0)
            {
                configuredPermIds = user.UserRoles
                    .SelectMany(ur => ur.Role.RolePermissions)
                    .Select(rp => rp.PermissionId)
                    .Distinct()
                    .ToList();
            }

            var effectiveNames = user.UserPermissions
                .Where(up => up.IsGranted)
                .Select(up => up.Permission.Name)
                .Concat(user.UserRoles.SelectMany(ur => ur.Role.RolePermissions).Select(rp => rp.Permission.Name))
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            return new SubAdminUserDetailDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                ConfiguredPermissionIds = configuredPermIds,
                EffectivePermissionNames = effectiveNames
            };
        }

        public async Task UpdateUserPermissionsAsync(UpdateUserPermissionsRequestDto dto)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                .FirstOrDefaultAsync(u => u.Id == dto.UserId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            // Remove existing user custom permissions
            _context.UserPermissions.RemoveRange(user.UserPermissions);

            if (dto.PermissionIds != null && dto.PermissionIds.Count > 0)
            {
                var distinctIds = dto.PermissionIds.Distinct().ToList();
                var validPermissionIds = await _context.Permissions
                    .Where(p => distinctIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                var newPerms = validPermissionIds.Select(pid => new UserPermission
                {
                    UserId = user.Id,
                    PermissionId = pid,
                    IsGranted = true,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.UserPermissions.AddRangeAsync(newPerms);
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
