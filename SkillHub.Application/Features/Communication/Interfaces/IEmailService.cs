using System.Threading.Tasks;

namespace SkillHub.Application.Features.Communication.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    Task SendTicketCreatedEmailAsync(string toEmail, string userName, string ticketNumber, string subject, string category);
    Task SendTicketReplyEmailAsync(string toEmail, string userName, string ticketNumber, string replyExcerpt, string senderName, bool isStaffReply);
    Task SendTicketStatusChangedEmailAsync(string toEmail, string userName, string ticketNumber, string newStatus);
}
