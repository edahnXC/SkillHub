using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillHub.Application.Features.Identity.Interfaces;
using SkillHub.Infrastructure.Authentication;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.DependencyInjection
{
    public static class ServiceRegistration
    {
        public static IServiceCollection
            AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(Options =>
            Options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IAuthService,AuthService>();
            return services;

        }
    }
}
