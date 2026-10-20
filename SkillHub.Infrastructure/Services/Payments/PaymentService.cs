using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SkillHub.Application.Features.Courses.Interfaces;
using SkillHub.Application.Features.Payments.DTOs;
using SkillHub.Application.Features.Payments.Interfaces;
using SkillHub.Infrastructure.Persistence.Context;
using Stripe;
using Stripe.Checkout;

namespace SkillHub.Infrastructure.Services.Payments;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IConfiguration _configuration;

    public PaymentService(
        ApplicationDbContext context,
        IEnrollmentService enrollmentService,
        IConfiguration configuration)
    {
        _context = context;
        _enrollmentService = enrollmentService;
        _configuration = configuration;
    }

    private string? GetStripeSecretKey()
    {
        return _configuration["Stripe:SecretKey"];
    }

    public async Task<CheckoutSessionResponseDto> CreateCheckoutSessionAsync(
        long studentId,
        CreateCheckoutSessionRequestDto dto)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == dto.CourseId);
        if (course == null)
        {
            throw new ArgumentException("Course not found");
        }

        var priceToCharge = course.DiscountPrice.HasValue && course.DiscountPrice.Value > 0
            ? course.DiscountPrice.Value
            : course.Price;

        var secretKey = GetStripeSecretKey();

        // If valid Stripe key is configured, attempt to create live Stripe Checkout Session
        if (!string.IsNullOrWhiteSpace(secretKey) && !secretKey.Contains("YOUR_STRIPE_SECRET_KEY"))
        {
            try
            {
                StripeConfiguration.ApiKey = secretKey;

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(priceToCharge * 100), // convert to paisa/cents integer
                                Currency = "inr",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = course.Title,
                                    Description = course.Description.Length > 200 
                                        ? course.Description.Substring(0, 197) + "..." 
                                        : course.Description,
                                    Images = !string.IsNullOrWhiteSpace(course.ThumbnailUrl) 
                                        ? new List<string> { course.ThumbnailUrl } 
                                        : null
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = dto.SuccessUrl.Contains("?") 
                        ? dto.SuccessUrl + "&session_id={CHECKOUT_SESSION_ID}" 
                        : dto.SuccessUrl + "?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = dto.CancelUrl,
                    Metadata = new Dictionary<string, string>
                    {
                        { "studentId", studentId.ToString() },
                        { "courseId", course.Id.ToString() }
                    }
                };

                var service = new SessionService();
                Session session = await service.CreateAsync(options);

                return new CheckoutSessionResponseDto(session.Url, session.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PaymentService] Stripe Checkout creation error ({ex.Message}). Falling back to Developer Mock Session.");
            }
        }

        // Developer Fallback / Test Mode when keys are not configured or invalid
        var mockSessionId = $"mock_session_{course.Id}_{studentId}_{Guid.NewGuid():N}";
        var mockSuccessUrl = dto.SuccessUrl.Contains("?") 
            ? dto.SuccessUrl + $"&session_id={mockSessionId}" 
            : dto.SuccessUrl + $"?session_id={mockSessionId}";

        return new CheckoutSessionResponseDto(mockSuccessUrl, mockSessionId);
    }

    public async Task<(bool Success, string Message, long? CourseId)> ConfirmPaymentAsync(
        long studentId,
        ConfirmPaymentRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.SessionId))
        {
            return (false, "Session ID is required", null);
        }

        long courseId = 0;

        // Handle Developer Fallback Session
        if (dto.SessionId.StartsWith("mock_session_"))
        {
            var parts = dto.SessionId.Split('_');
            if (parts.Length >= 3 && long.TryParse(parts[2], out var parsedCourseId))
            {
                courseId = parsedCourseId;
            }
        }
        else
        {
            // Verify with Stripe API
            var secretKey = GetStripeSecretKey();
            if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Contains("YOUR_STRIPE_SECRET_KEY"))
            {
                return (false, "Stripe secret key configuration is missing", null);
            }

            try
            {
                StripeConfiguration.ApiKey = secretKey;
                var service = new SessionService();
                Session session = await service.GetAsync(dto.SessionId);

                if (session == null || session.PaymentStatus != "paid")
                {
                    return (false, "Payment has not been completed or verified", null);
                }

                if (session.Metadata != null && session.Metadata.TryGetValue("courseId", out var courseIdStr))
                {
                    long.TryParse(courseIdStr, out courseId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PaymentService] Stripe Verification error ({ex.Message})");
                return (false, $"Payment verification failed: {ex.Message}", null);
            }
        }

        if (courseId == 0)
        {
            return (false, "Could not determine course for session", null);
        }

        // Check if already enrolled
        var isEnrolled = await _enrollmentService.IsEnrolledAsync(studentId, courseId);
        if (isEnrolled)
        {
            return (true, "Already enrolled in this course", courseId);
        }

        // Enroll student
        var enrolled = await _enrollmentService.EnrollStudentAsync(studentId, courseId);
        if (!enrolled)
        {
            return (false, "Failed to complete course enrollment", courseId);
        }

        return (true, "Payment confirmed and enrolled successfully", courseId);
    }
}
