using System.Threading.Tasks;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Seed
{
    public static class CourseSeeder
    {
        public static Task SeedCoursesAsync(ApplicationDbContext context)
        {
            // Sample course seeding is permanently disabled so that courses deleted or managed by admins/instructors are never recreated on server restart.
            return Task.CompletedTask;
        }
    }
}
