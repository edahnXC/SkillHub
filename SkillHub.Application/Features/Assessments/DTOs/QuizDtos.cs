using System;
using System.Collections.Generic;

namespace SkillHub.Application.Features.Assessments.DTOs
{
    public class QuizDto
    {
        public long Id { get; set; }
        public long CourseId { get; set; }
        public long? SectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PassingScorePercentage { get; set; }
        public int TotalQuestions { get; set; }
        public List<QuizQuestionDto> Questions { get; set; } = new();
        public QuizAttemptResultDto? BestAttempt { get; set; }
    }

    public class QuizQuestionDto
    {
        public long Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public List<QuizOptionDto> Options { get; set; } = new();
    }

    public class QuizOptionDto
    {
        public long Id { get; set; }
        public string OptionText { get; set; } = string.Empty;
    }

    public class SubmitQuizAttemptRequestDto
    {
        public long QuizId { get; set; }
        public List<SubmitQuestionAnswerDto> Answers { get; set; } = new();
    }

    public class SubmitQuestionAnswerDto
    {
        public long QuestionId { get; set; }
        public long SelectedOptionId { get; set; }
    }

    public class QuizAttemptResultDto
    {
        public long AttemptId { get; set; }
        public long QuizId { get; set; }
        public decimal ScorePercentage { get; set; }
        public int CorrectCount { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsPassed { get; set; }
        public int PassingScorePercentage { get; set; }
        public DateTime CompletedAt { get; set; }
        public List<QuestionFeedbackDto> QuestionFeedback { get; set; } = new();
        public bool IsCertificateAwarded { get; set; }
        public SkillHub.Application.Features.Learning.DTOs.CertificateDto? Certificate { get; set; }
    }

    public class QuestionFeedbackDto
    {
        public long QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public long SelectedOptionId { get; set; }
        public long CorrectOptionId { get; set; }
        public bool IsCorrect { get; set; }
        public string? Explanation { get; set; }
    }

    public class CreateQuizRequestDto
    {
        public long CourseId { get; set; }
        public long? SectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PassingScorePercentage { get; set; } = 70;
        public List<CreateQuizQuestionDto> Questions { get; set; } = new();
    }

    public class CreateQuizQuestionDto
    {
        public string QuestionText { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public int DisplayOrder { get; set; }
        public List<CreateQuizOptionDto> Options { get; set; } = new();
    }

    public class CreateQuizOptionDto
    {
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
