using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkillHub.Application.Features.Communication.Interfaces;

namespace SkillHub.Infrastructure.Services.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(toEmail)) return;

        if (string.Equals(_settings.Provider, "Smtp", StringComparison.OrdinalIgnoreCase) 
            && !string.IsNullOrWhiteSpace(_settings.Smtp.UserName) 
            && !string.IsNullOrWhiteSpace(_settings.Smtp.Password))
        {
            try
            {
                using var client = new SmtpClient(_settings.Smtp.Host, _settings.Smtp.Port)
                {
                    EnableSsl = _settings.Smtp.EnableSsl,
                    Credentials = new NetworkCredential(_settings.Smtp.UserName, _settings.Smtp.Password)
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                _logger.LogInformation("✅ [SMTP EMAIL SENT] To: {To}, Subject: {Subject}", toEmail, subject);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ SMTP send failed. Falling back to Console Logger mode.");
            }
        }

        // 100% Free Development Logger Mode (Formatted terminal output)
        _logger.LogInformation(@"
╔══════════════════════════════════════════════════════════════════════════════════════╗
║ 📬 [SKILLHUB FREE EMAIL ENGINE - TRANSACTIONAL DISPATCH]                             ║
╠══════════════════════════════════════════════════════════════════════════════════════╣
║ To:      {ToEmail,-74} ║
║ Subject: {Subject,-74} ║
║ Sender:  {SenderName} <{SenderEmail}>
╠══════════════════════════════════════════════════════════════════════════════════════╣
║ Preview:                                                                             ║
║ {PreviewText}
╚══════════════════════════════════════════════════════════════════════════════════════╝",
            toEmail, subject, _settings.SenderName, _settings.SenderEmail, GetPlainTextPreview(htmlBody));

        await Task.CompletedTask;
    }

    public async Task SendTicketCreatedEmailAsync(string toEmail, string userName, string ticketNumber, string subject, string category)
    {
        var emailSubject = $"[SkillHub Support] Request Received: {ticketNumber} - {subject}";
        var html = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<style>
  body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background: #f8fafc; margin: 0; padding: 24px; color: #1e293b; }}
  .card {{ max-width: 580px; margin: 0 auto; background: #ffffff; border-radius: 12px; border: 1px solid #e2e8f0; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); }}
  .header {{ background: linear-gradient(135deg, #4f46e5, #7c3aed); padding: 28px; text-align: center; color: #ffffff; }}
  .header h1 {{ margin: 0; font-size: 22px; font-weight: 800; letter-spacing: -0.02em; }}
  .content {{ padding: 28px; }}
  .badge {{ display: inline-block; padding: 4px 10px; border-radius: 9999px; font-size: 12px; font-weight: 700; background: #e0e7ff; color: #4338ca; margin-bottom: 16px; }}
  .ticket-box {{ background: #f1f5f9; border-radius: 8px; padding: 16px; margin: 16px 0; border-left: 4px solid #4f46e5; }}
  .footer {{ background: #f8fafc; padding: 16px; text-align: center; font-size: 12px; color: #64748b; border-top: 1px solid #e2e8f0; }}
</style>
</head>
<body>
  <div class='card'>
    <div class='header'>
      <h1>🛡️ SkillHub Helpdesk</h1>
    </div>
    <div class='content'>
      <span class='badge'>{category}</span>
      <p>Hi <strong>{userName}</strong>,</p>
      <p>Your support ticket has been registered successfully. Our support staff has been notified and is reviewing your inquiry.</p>
      <div class='ticket-box'>
        <div style='font-size: 13px; color: #64748b;'>Ticket Reference:</div>
        <div style='font-size: 18px; font-weight: 800; color: #1e293b; margin-top: 2px;'>{ticketNumber}</div>
        <div style='font-size: 14px; font-weight: 600; color: #334155; margin-top: 6px;'>{subject}</div>
      </div>
      <p>We aim to respond to all inquiries within <strong>24 hours</strong>. You can check updates or add more details by visiting your Support portal.</p>
    </div>
    <div class='footer'>
      SkillHub Learning Platform &bull; Automated Support Dispatcher
    </div>
  </div>
</body>
</html>";

        await SendEmailAsync(toEmail, emailSubject, html);
    }

    public async Task SendTicketReplyEmailAsync(string toEmail, string userName, string ticketNumber, string replyExcerpt, string senderName, bool isStaffReply)
    {
        var senderLabel = isStaffReply ? "Support Staff" : senderName;
        var emailSubject = $"[Update] New response on ticket {ticketNumber}";
        var html = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<style>
  body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background: #f8fafc; margin: 0; padding: 24px; color: #1e293b; }}
  .card {{ max-width: 580px; margin: 0 auto; background: #ffffff; border-radius: 12px; border: 1px solid #e2e8f0; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); }}
  .header {{ background: linear-gradient(135deg, #0ea5e9, #4f46e5); padding: 24px; text-align: center; color: #ffffff; }}
  .content {{ padding: 28px; }}
  .message-quote {{ background: #f8fafc; border-radius: 8px; border: 1px solid #e2e8f0; border-left: 4px solid #0ea5e9; padding: 16px; margin: 16px 0; font-style: italic; color: #334155; }}
  .footer {{ background: #f8fafc; padding: 16px; text-align: center; font-size: 12px; color: #64748b; border-top: 1px solid #e2e8f0; }}
</style>
</head>
<body>
  <div class='card'>
    <div class='header'>
      <h2 style='margin:0; font-size: 20px;'>💬 Ticket Response Notification</h2>
    </div>
    <div class='content'>
      <p>Hi <strong>{userName}</strong>,</p>
      <p><strong>{senderLabel}</strong> posted an update regarding your ticket <strong>{ticketNumber}</strong>:</p>
      <div class='message-quote'>
        ""{replyExcerpt}""
      </div>
      <p>To view the full thread and reply, please log in to your SkillHub account.</p>
    </div>
    <div class='footer'>
      SkillHub Learning Platform &bull; Automated Support Dispatcher
    </div>
  </div>
</body>
</html>";

        await SendEmailAsync(toEmail, emailSubject, html);
    }

    public async Task SendTicketStatusChangedEmailAsync(string toEmail, string userName, string ticketNumber, string newStatus)
    {
        var emailSubject = $"[Status Update] Ticket {ticketNumber} is now {newStatus}";
        var html = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<style>
  body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background: #f8fafc; margin: 0; padding: 24px; color: #1e293b; }}
  .card {{ max-width: 580px; margin: 0 auto; background: #ffffff; border-radius: 12px; border: 1px solid #e2e8f0; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); }}
  .header {{ background: linear-gradient(135deg, #10b981, #059669); padding: 24px; text-align: center; color: #ffffff; }}
  .content {{ padding: 28px; }}
  .status-badge {{ display: inline-block; padding: 6px 14px; border-radius: 8px; font-weight: 800; background: #d1fae5; color: #065f46; font-size: 14px; margin: 12px 0; }}
  .footer {{ background: #f8fafc; padding: 16px; text-align: center; font-size: 12px; color: #64748b; border-top: 1px solid #e2e8f0; }}
</style>
</head>
<body>
  <div class='card'>
    <div class='header'>
      <h2 style='margin:0; font-size: 20px;'>✨ Ticket Status Updated</h2>
    </div>
    <div class='content'>
      <p>Hi <strong>{userName}</strong>,</p>
      <p>The status of your ticket <strong>{ticketNumber}</strong> has been updated to:</p>
      <div class='status-badge'>{newStatus.ToUpperInvariant()}</div>
      <p>If your issue is resolved, no further action is required. If you need more help, you can reply directly from your SkillHub dashboard.</p>
    </div>
    <div class='footer'>
      SkillHub Learning Platform &bull; Automated Support Dispatcher
    </div>
  </div>
</body>
</html>";

        await SendEmailAsync(toEmail, emailSubject, html);
    }

    private static string GetPlainTextPreview(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;
        var plain = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        plain = System.Text.RegularExpressions.Regex.Replace(plain, @"\s+", " ").Trim();
        return plain.Length > 200 ? plain.Substring(0, 200) + "..." : plain;
    }
}
