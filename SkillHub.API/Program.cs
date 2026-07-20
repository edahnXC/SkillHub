using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using SkillHub.Infrastructure.DependencyInjection;
using SkillHub.Infrastructure.Persistence.Context;
using SkillHub.Infrastructure.Seed;
using SkillHub.Application.Mappings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["JwtSettings:Issuer"],

                ValidAudience =
                    builder.Configuration["JwtSettings:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["JwtSettings:Key"]!))
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(
    builder.Configuration);

// AutoMapper — scans Application assembly for all Profile classes
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

var app = builder.Build();


// ==============================
// Seed Default Roles
// ==============================

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await context.Database.MigrateAsync();

    await RoleSeeder.SeedRolesAsync(context);

    await PermissionSeeder.SeedPermissionsAsync(context);

    await UserSeeder.SeedUsersAsync(context);

    await CourseCategorySeeder.SeedCategoriesAsync(context);
}

// ==============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors("AngularPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
//addmappers