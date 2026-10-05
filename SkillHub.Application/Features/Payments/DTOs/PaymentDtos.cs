namespace SkillHub.Application.Features.Payments.DTOs;

public record CreateCheckoutSessionRequestDto(
    long CourseId,
    string SuccessUrl,
    string CancelUrl
);

public record CheckoutSessionResponseDto(
    string SessionUrl,
    string SessionId
);

public record ConfirmPaymentRequestDto(
    string SessionId
);
