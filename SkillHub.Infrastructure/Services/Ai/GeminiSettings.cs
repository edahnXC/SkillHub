namespace SkillHub.Infrastructure.Services.Ai;

public class GeminiSettings
{
    public const string SectionName = "GeminiSettings";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-1.5-flash";
    public int MaxOutputTokens { get; set; } = 1024;
    public double Temperature { get; set; } = 0.7;
}
