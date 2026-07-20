using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Features.Courses.DTOs;
using SkillHub.Application.Features.Courses.Interfaces;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Services.Courses;

public class CourseCategoryService : ICourseCategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CourseCategoryService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CourseCategoryDto>> GetAllCategoriesAsync()
    {
        return await _context.CourseCategories
            .OrderBy(c => c.Name)
            .ProjectTo<CourseCategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
}
