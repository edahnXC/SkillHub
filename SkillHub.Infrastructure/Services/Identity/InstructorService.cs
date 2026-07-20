using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Features.Identity.DTOs;
using SkillHub.Application.Features.Identity.Interfaces;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Services.Identity;

public class InstructorService : IInstructorService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public InstructorService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<InstructorDto>> SearchInstructorsAsync(string? query)
    {
        var instructorsQuery = _context.Users
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Instructor"))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var trimmed = query.Trim();

            if (long.TryParse(trimmed, out var id))
            {
                instructorsQuery = instructorsQuery.Where(u => u.Id == id);
            }
            else
            {
                var lower = trimmed.ToLower();
                instructorsQuery = instructorsQuery
                    .Where(u =>
                        u.FirstName.ToLower().Contains(lower) ||
                        u.LastName.ToLower().Contains(lower) ||
                        u.Email.ToLower().Contains(lower));
            }
        }

        return await instructorsQuery
            .ProjectTo<InstructorDto>(_mapper.ConfigurationProvider)
            .OrderBy(i => i.FirstName)
            .ThenBy(i => i.LastName)
            .ToListAsync();
    }

    public async Task<InstructorDto?> GetInstructorByIdAsync(long id)
    {
        return await _context.Users
            .Where(u => u.Id == id && u.UserRoles.Any(ur => ur.Role.Name == "Instructor"))
            .ProjectTo<InstructorDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }
}
