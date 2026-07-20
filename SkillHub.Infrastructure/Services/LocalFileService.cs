using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SkillHub.Application.Features.Files.Interfaces;

namespace SkillHub.Infrastructure.Services;

public class LocalFileService : IFileService
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalFileService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string directoryName)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File cannot be empty.");
        }

        // Only allow image files
        if (!file.ContentType.StartsWith("image/"))
        {
            throw new ArgumentException("Only image files are allowed.");
        }

        // Validate extension
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        if (!allowedExtensions.Contains(ext))
        {
            throw new ArgumentException("Invalid file extension.");
        }

        var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadPath = Path.Combine(webRootPath, "uploads", directoryName);

        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var uniqueFileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
        {
            throw new InvalidOperationException("HttpContext is null");
        }

        var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
        var fileUrl = $"{baseUrl}/uploads/{directoryName}/{uniqueFileName}";

        return fileUrl;
    }
}
