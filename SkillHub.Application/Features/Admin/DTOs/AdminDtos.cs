namespace SkillHub.Application.Features.Admin.DTOs;

public class AdminUserDto
{
    public long Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class AdminCourseDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}
