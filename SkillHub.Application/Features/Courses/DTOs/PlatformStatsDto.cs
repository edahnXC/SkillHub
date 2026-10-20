namespace SkillHub.Application.Features.Courses.DTOs;

public class PlatformStatsDto
{
    public int CoursesCount { get; set; }
    public int InstructorsCount { get; set; }
    public int StudentsCount { get; set; }
    public int TotalEnrollments { get; set; }
    public double AverageRating { get; set; }
}
