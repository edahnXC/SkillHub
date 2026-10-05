using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Features.Courses.DTOs;
using SkillHub.Application.Features.Courses.Interfaces;
using SkillHub.Domain.Entities.Courses;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Services.Courses;

public class CourseService : ICourseService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CourseService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CourseResponseDto> CreateAsync(
        CreateCourseRequestDto request, long instructorId)
    {
        var course = _mapper.Map<Course>(request);
        course.InstructorId = instructorId;

        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(course.Id)
               ?? throw new Exception("Course creation failed.");
    }

    public async Task<List<CourseListDto>> GetAllAsync()
    {
        return await _context.Courses
            .Where(c => c.IsPublished)
            .OrderByDescending(c => c.CreatedAt)
            .ProjectTo<CourseListDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<CourseResponseDto?> GetByIdAsync(long id)
    {
        return await _context.Courses
            .Include(x => x.Category)
            .Include(x => x.Instructor)
            .Where(x => x.Id == id)
            .ProjectTo<CourseResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    public async Task<List<CourseListDto>> GetCoursesByInstructorAsync(long instructorId)
    {
        return await _context.Courses
            .Where(x => x.InstructorId == instructorId)
            .ProjectTo<CourseListDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task UpdateAsync(
        UpdateCourseRequestDto request, long instructorId)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(x => x.Id == request.Id);

        if (course == null)
            throw new Exception("Course not found.");

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You are not authorized to edit this course.");

        _mapper.Map(request, course); // maps DTO fields onto the tracked entity
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id, long instructorId)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(x => x.Id == id);

        if (course == null)
            throw new Exception("Course not found.");

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You are not authorized to delete this course.");

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
    }
}