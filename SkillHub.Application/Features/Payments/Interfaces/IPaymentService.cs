using SkillHub.Application.Features.Payments.DTOs;

namespace SkillHub.Application.Features.Payments.Interfaces;

public interface IPaymentService
{
    Task<CheckoutSessionResponseDto> CreateCheckoutSessionAsync(
        long studentId,
        CreateCheckoutSessionRequestDto dto
    );

    Task<(bool Success, string Message, long? CourseId)> ConfirmPaymentAsync(
        long studentId,
        ConfirmPaymentRequestDto dto
    );
}
