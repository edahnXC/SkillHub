using FluentValidation;

using SkillHub.Application.Features.Courses.DTOs;

namespace SkillHub.Application.Features.Courses.Validators;

public class CreateCourseValidator
    : AbstractValidator<CreateCourseRequestDto>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty();

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0);
    }
}