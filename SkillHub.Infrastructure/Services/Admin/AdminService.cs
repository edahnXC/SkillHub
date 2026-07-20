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
}
