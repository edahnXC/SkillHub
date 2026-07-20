using AutoMapper;
using SkillHub.Application.Features.Admin.DTOs;
using SkillHub.Application.Features.Courses.DTOs;
using SkillHub.Application.Features.Identity.DTOs;
using SkillHub.Domain.Entities.Courses;
using SkillHub.Domain.Entities.Identity;

namespace SkillHub.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── Course Category ──────────────────────────
        CreateMap<CourseCategory, CourseCategoryDto>().ReverseMap();

        // ── Enrollment → CourseListDto (for dashboard) 
        CreateMap<Enrollment, CourseListDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Course.Id))
            .ForMember(d => d.Title, o => o.MapFrom(s => s.Course.Title))
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Course.Price))
            .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(s => s.Course.ThumbnailUrl))
            .ForMember(d => d.IsPublished, o => o.MapFrom(s => s.Course.IsPublished))
            .ForMember(d => d.InstructorId, o => o.MapFrom(s => s.Course.InstructorId));

        // ── Course → CourseListDto (flat, for grids) ─
        CreateMap<Course, CourseListDto>();

        // ── Course → CourseResponseDto (detail view) ─
        CreateMap<Course, CourseResponseDto>()
            .ForMember(d => d.CategoryName,
                       o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.InstructorName,
                       o => o.MapFrom(s => s.Instructor.FirstName + " " + s.Instructor.LastName));

        // ── CreateCourseRequestDto → Course ──────────
        CreateMap<CreateCourseRequestDto, Course>()
            .ForMember(d => d.IsPublished, o => o.MapFrom(_ => false));

        // ── UpdateCourseRequestDto → Course ──────────
        CreateMap<UpdateCourseRequestDto, Course>()
            .ForMember(d => d.Id, o => o.Ignore()); // never overwrite the PK

        // ── User → InstructorDto ─────────────────────
        CreateMap<User, InstructorDto>()
            .ForMember(d => d.CourseCount,
                       o => o.MapFrom(s => s.Courses.Count));

        // ── User → AdminUserDto ──────────────────────
        CreateMap<User, AdminUserDto>()
            .ForMember(d => d.Roles,
                       o => o.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name).ToList()));

        // ── Course → AdminCourseDto ──────────────────
        CreateMap<Course, AdminCourseDto>()
            .ForMember(d => d.InstructorName,
                       o => o.MapFrom(s => s.Instructor.FirstName + " " + s.Instructor.LastName))
            .ForMember(d => d.CategoryName,
                       o => o.MapFrom(s => s.Category.Name));
    }
}
