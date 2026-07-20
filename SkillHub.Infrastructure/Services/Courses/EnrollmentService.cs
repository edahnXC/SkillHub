using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Features.Courses.DTOs;
using SkillHub.Application.Features.Courses.Interfaces;
using SkillHub.Domain.Entities.Courses;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Services.Courses;

public class EnrollmentService : IEnrollmentService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public EnrollmentService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<bool> EnrollStudentAsync(long studentId, long courseId)
    {
        // Check if the student is already enrolled
        var exists = await IsEnrolledAsync(studentId, courseId);
        if (exists)
        {
            return false;
        }

        // Verify the course exists
        var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists)
        {
            throw new ArgumentException("Course not found.");
        }

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow,
            IsActive = true
        };

        await _context.Enrollments.AddAsync(enrollment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<CourseListDto>> GetStudentEnrollmentsAsync(long studentId)
    {
        return await _context.Enrollments
            .Where(e => e.StudentId == studentId && e.IsActive)
            .ProjectTo<CourseListDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<bool> IsEnrolledAsync(long studentId, long courseId)
    {
        return await _context.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId && e.IsActive);
    }
}
