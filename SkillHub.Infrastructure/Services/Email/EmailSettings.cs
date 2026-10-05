namespace SkillHub.Infrastructure.Services.Email;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    // "Console" or "Smtp"
    public string Provider { get; set; } = "Console";
    public string SenderName { get; set; } = "SkillHub Support";
    public string SenderEmail { get; set; } = "support@skillhub.com";
    
    public SmtpConfig Smtp { get; set; } = new();
}

public class SmtpConfig
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
