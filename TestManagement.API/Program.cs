using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TestManagement.BAL.Hubs;
using TestManagement.BAL.Services;
using TestManagement.BAL.Services.Interfaces;
using TestManagement.BAL.Settings;
using TestManagement.DAL.Data;
using TestManagement.DAL.Repositories;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

        builder.Services.AddSignalR();

        var clientOrigin = builder.Configuration["ClientOrigin"] ?? "https://localhost:7097";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("ClientCors", policy =>
                policy.WithOrigins(clientOrigin)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials());
        });

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
        builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
        builder.Services.AddScoped<ITopicRepository, TopicRepository>();
        builder.Services.AddScoped<IClassRepository, ClassRepository>();
        builder.Services.AddScoped<IExamRepository, ExamRepository>();
        builder.Services.AddScoped<IExamAssignmentRepository, ExamAssignmentRepository>();
        builder.Services.AddScoped<IExamAttemptRepository, ExamAttemptRepository>();
        builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ISubjectService, SubjectService>();
        builder.Services.AddScoped<IQuestionService, QuestionService>();
        builder.Services.AddScoped<ITopicService, TopicService>();
        builder.Services.AddScoped<IClassService, ClassService>();
        builder.Services.AddScoped<IExamService, ExamService>();
        builder.Services.AddScoped<IExamAssignmentService, ExamAssignmentService>();
        builder.Services.AddScoped<IExamAttemptService, ExamAttemptService>();
        builder.Services.AddScoped<IExamResultService, ExamResultService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();

        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = signingKey,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };

                // WebSocket không gửi Authorization header → đọc token từ query string khi gọi Hub
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();

        builder.Services.AddControllers().AddOData(options =>
        {
            options.Select()
                .Filter()
                .OrderBy()
                .Count()
                .SetMaxTop(100);
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Nhập JWT Bearer token."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("ClientCors");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHub<NotificationHub>("/hubs/notifications");
        app.MapHub<ExamMonitorHub>("/hubs/exam-monitor");
        app.Run();
    }
}
