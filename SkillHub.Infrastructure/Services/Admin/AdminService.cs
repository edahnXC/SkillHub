using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Features.Admin.DTOs;
using SkillHub.Application.Features.Admin.Interfaces;
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
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ProjectTo<AdminUserDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task DeleteUserAsync(long id)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
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

        // Restrict role creation to SubAdmin, Instructor, or Student (never allow creating SuperAdmins administratively)
        if (dto.Role != "SubAdmin" && dto.Role != "Instructor" && dto.Role != "Student")
        {
            throw new Exception("Invalid role specified.");
        }

        var user = new SkillHub.Domain.Entities.Identity.User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = true,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        user.UserRoles.Add(new SkillHub.Domain.Entities.Identity.UserRole { RoleId = role.Id, User = user });
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
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
