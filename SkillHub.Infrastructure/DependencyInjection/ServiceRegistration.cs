using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillHub.Application.Features.Identity.Interfaces;
using SkillHub.Application.Features.Identity.Models;
using SkillHub.Infrastructure.Authentication;
using SkillHub.Infrastructure.Persistence.Context;
using SkillHub.Application.Features.Courses.Interfaces;
using SkillHub.Infrastructure.Services.Courses;
using SkillHub.Application.Features.Files.Interfaces;
using SkillHub.Infrastructure.Services;
using SkillHub.Infrastructure.Services.Identity;
using SkillHub.Application.Features.Admin.Interfaces;
using SkillHub.Infrastructure.Services.Admin;
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
            services.AddScoped<
                ICourseService,
                CourseService>();

            services.AddHttpContextAccessor();
            services.AddScoped<IFileService, LocalFileService>();
            services.AddScoped<ICourseCategoryService, CourseCategoryService>();
            services.AddScoped<IInstructorService, InstructorService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IEnrollmentService, EnrollmentService>();

            // Permission-based authorization
            services.AddScoped<
                IAuthorizationHandler,
                PermissionAuthorizationHandler>();

            services.AddSingleton<
                IAuthorizationPolicyProvider,
                PermissionPolicyProvider>();

            return services;


        }
    }
}
