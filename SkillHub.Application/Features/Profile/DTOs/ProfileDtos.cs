namespace SkillHub.Application.Features.Profile.DTOs;

public record ProfileResponseDto(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? ProfileImageUrl,
    List<string> Roles,
    DateTime CreatedAt
);

public record UpdateProfileRequestDto(
    string FirstName,
    string LastName,
    string? PhoneNumber
);

public record ChangePasswordRequestDto(
    string CurrentPassword,
    string NewPassword
);
