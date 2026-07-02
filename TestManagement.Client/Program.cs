using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using TestManagement.Client.Configuration;
using TestManagement.Client.Services;

namespace TestManagement.Client;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(nameof(ApiSettings)));
        builder.Services.AddControllersWithViews();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(120);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.AccessDeniedPath = "/Auth/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Events.OnValidatePrincipal = context =>
                {
                    if (string.IsNullOrWhiteSpace(context.HttpContext.Session.GetString(SessionKeys.AccessToken)))
                    {
                        context.RejectPrincipal();
                    }

                    return Task.CompletedTask;
                };
            })
            .AddCookie("ExternalCookie", options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

        // Chỉ đăng ký Google OAuth khi đã cấu hình ClientId/ClientSecret,
        // tránh lỗi "The 'ClientId' option must be provided" lúc khởi động khi để trống.
        var googleClientId = builder.Configuration["Google:ClientId"];
        var googleClientSecret = builder.Configuration["Google:ClientSecret"];
        if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
        {
            builder.Services.AddAuthentication().AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
                options.CallbackPath = "/Auth/GoogleCallback";
                options.SignInScheme = "ExternalCookie";
                options.SaveTokens = false;
            });
        }

        builder.Services.AddAuthorization();
        builder.Services.AddHttpClient<ApiClient>((serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl);
        });

        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<SubjectService>();
        builder.Services.AddScoped<TopicService>();
        builder.Services.AddScoped<QuestionService>();
        builder.Services.AddScoped<ClassService>();
        builder.Services.AddScoped<ClassService>();
        builder.Services.AddScoped<ExamService>();
        builder.Services.AddScoped<ExamAttemptService>();
        builder.Services.AddScoped<ExamResultService>();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
