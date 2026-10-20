using System;
using System.Collections.Generic;
using SkillHub.Application.Features.Courses.DTOs;

namespace SkillHub.Application.Features.Learning.DTOs
{
    public class CourseLearningDto
    {
        public long CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public decimal ProgressPercentage { get; set; }
        public int CompletedLessonsCount { get; set; }
        public int TotalLessonsCount { get; set; }
        public List<LearningSectionDto> Sections { get; set; } = new();
        public CertificateDto? Certificate { get; set; }
        public bool IsCertificateEligible { get; set; }
        public string? CertificateRequirementMessage { get; set; }
        public int TotalQuizzesCount { get; set; }
        public int PassedQuizzesCount { get; set; }
        public decimal? AverageAssessmentScore { get; set; }
    }

    public class LearningSectionDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public List<LearningLessonDto> Lessons { get; set; } = new();
    }

    public class LearningLessonDto
    {
        public long Id { get; set; }
        public long SectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<LessonResourceDto> Resources { get; set; } = new();
    }

    public class CertificateDto
    {
        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public decimal? FinalScore { get; set; }
        public string? Grade { get; set; }
    }

    public class ToggleLessonCompletionRequestDto
    {
        public long CourseId { get; set; }
        public long LessonId { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class LessonCompletionResultDto
    {
        public long LessonId { get; set; }
        public bool IsCompleted { get; set; }
        public decimal UpdatedProgressPercentage { get; set; }
        public int CompletedLessonsCount { get; set; }
        public int TotalLessonsCount { get; set; }
        public bool IsCourseCompleted { get; set; }
        public CertificateDto? Certificate { get; set; }
        public bool IsCertificateEligible { get; set; }
        public string? CertificateRequirementMessage { get; set; }
    }
}
