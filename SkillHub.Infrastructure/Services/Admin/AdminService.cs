using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Features.Admin.DTOs;
using SkillHub.Application.Features.Admin.Interfaces;
using SkillHub.Domain.Entities.Identity;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Services.Admin;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AdminService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();

        return users.Select(u => new AdminUserDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            Permissions = u.UserPermissions.Where(up => up.IsGranted).Select(up => up.Permission.Name).ToList()
        });
    }

    public async Task DeleteUserAsync(long id)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .Include(u => u.UserPermissions)
            .Include(u => u.RefreshTokens)
            .Include(u => u.Courses)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<AdminCourseDto>> GetAllCoursesAsync()
    {
        return await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .OrderByDescending(c => c.CreatedAt)
            .ProjectTo<AdminCourseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task DeleteCourseAsync(long id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }
    }

    public async Task CreateUserAsync(CreateUserRequestDto dto)
    {
        var existing = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (existing)
        {
            throw new Exception("Email already exists.");
        }

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role);
        if (role == null)
        {
            throw new Exception($"Role '{dto.Role}' not found.");
        }

        if (dto.Role != "SubAdmin" && dto.Role != "Instructor" && dto.Role != "Student")
        {
            throw new Exception("Invalid role specified.");
        }

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = true,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        user.UserRoles.Add(new UserRole { RoleId = role.Id, User = user });
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // If specific permissions were selected for this SubAdmin, save them to UserPermissions
        if (dto.PermissionIds != null && dto.PermissionIds.Count > 0)
        {
            var distinctIds = dto.PermissionIds.Distinct().ToList();
            var validIds = await _context.Permissions
                .Where(p => distinctIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            var userPerms = validIds.Select(pid => new UserPermission
            {
                UserId = user.Id,
                PermissionId = pid,
                IsGranted = true,
                CreatedAt = DateTime.UtcNow
            });

            await _context.UserPermissions.AddRangeAsync(userPerms);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ApproveCourseAsync(long id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            throw new Exception("Course not found.");
        }

        course.IsPublished = true;
        await _context.SaveChangesAsync();
    }
}
