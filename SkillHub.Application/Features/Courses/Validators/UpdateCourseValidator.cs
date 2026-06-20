using FluentValidation;

using SkillHub.Application.Features.Courses.DTOs;

namespace SkillHub.Application.Features.Courses.Validators;

public class UpdateCourseValidator
    : AbstractValidator<UpdateCourseRequestDto>
{
    public UpdateCourseValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThan(0);
    }
}