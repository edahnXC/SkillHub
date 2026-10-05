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
using SkillHub.Application.Features.Payments.Interfaces;
using SkillHub.Infrastructure.Services.Payments;
using SkillHub.Application.Features.Profile.Interfaces;
using SkillHub.Application.Features.Admin.Interfaces;
using SkillHub.Infrastructure.Services.Admin;
using SkillHub.Application.Features.Learning.Interfaces;
using SkillHub.Infrastructure.Services.Learning;
using SkillHub.Application.Features.Reviews.Interfaces;
using SkillHub.Infrastructure.Services.Reviews;
using SkillHub.Application.Features.Assessments.Interfaces;
using SkillHub.Infrastructure.Services.Assessments;

namespace SkillHub.Infrastructure.DependencyInjection
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.Configure<JwtSettings>(
                configuration.GetSection("JwtSettings"));

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<ICurriculumService, CurriculumService>();
            services.AddScoped<ILearningService, LearningService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IQuizService, QuizService>();
            services.AddScoped<IPermissionService, PermissionService>();

            // Free Email & Support Helpdesk Engine
            services.Configure<SkillHub.Infrastructure.Services.Email.EmailSettings>(
                configuration.GetSection(SkillHub.Infrastructure.Services.Email.EmailSettings.SectionName));
            services.AddScoped<SkillHub.Application.Features.Communication.Interfaces.IEmailService, SkillHub.Infrastructure.Services.Email.EmailService>();
            services.AddScoped<SkillHub.Application.Features.Support.Interfaces.ISupportService, SkillHub.Infrastructure.Services.Support.SupportService>();

            // Google Gemini AI Learning Copilot
            services.Configure<SkillHub.Infrastructure.Services.Ai.GeminiSettings>(
                configuration.GetSection(SkillHub.Infrastructure.Services.Ai.GeminiSettings.SectionName));
            services.AddHttpClient<SkillHub.Application.Features.Ai.Interfaces.IAiChatService, SkillHub.Infrastructure.Services.Ai.GeminiAiService>();

            services.AddHttpContextAccessor();
            services.AddScoped<IFileService, LocalFileService>();
            services.AddScoped<ICourseCategoryService, CourseCategoryService>();
            services.AddScoped<IInstructorService, InstructorService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IEnrollmentService, EnrollmentService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IPaymentService, PaymentService>();

            // Permission-based authorization
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            return services;
        }
    }
}
