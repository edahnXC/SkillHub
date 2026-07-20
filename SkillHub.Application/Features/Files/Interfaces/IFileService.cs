using Microsoft.AspNetCore.Http;

namespace SkillHub.Application.Features.Files.Interfaces;

public interface IFileService
{
    Task<string> UploadImageAsync(IFormFile file, string directoryName);
}
