using SkillHub.Domain.Entities.Identity;
namespace SkillHub.Application.Interfaces.Persistence;
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task SaveCHangesAsync();
}