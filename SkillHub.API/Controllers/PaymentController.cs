using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Features.Payments.DTOs;
using SkillHub.Application.Features.Payments.Interfaces;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Student")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    private long? GetStudentId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var studentId) ? studentId : null;
    }

    [HttpPost("create-checkout")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequestDto dto)
    {
        var studentId = GetStudentId();
        if (studentId == null) return Unauthorized("Student ID not found in claims");

        try
        {
            var result = await _paymentService.CreateCheckoutSessionAsync(studentId.Value, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequestDto dto)
    {
        var studentId = GetStudentId();
        if (studentId == null) return Unauthorized("Student ID not found in claims");

        var (success, message, courseId) = await _paymentService.ConfirmPaymentAsync(studentId.Value, dto);
        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new { message, courseId });
    }
}
