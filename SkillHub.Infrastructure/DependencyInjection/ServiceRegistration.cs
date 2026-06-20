using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillHub.Application.Features.Identity.Interfaces;
using SkillHub.Application.Features.Identity.Models;
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

            services.Configure<JwtSettings>(
                configuration.GetSection(
                    "JwtSettings")); //maps the details to the settings

            services.AddScoped<IAuthService, AuthService>(); //whenever asp.net core see iauthservice it creates authservice
            
            services.AddScoped<IJwtTokenGenerator,
                JwtTokenGenerator>();
            return services;



        }
    }
}
