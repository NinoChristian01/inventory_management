using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.WebEncoders;
using Serilog;
using ServiceDesk.Application.Interfaces;
using ServiceDesk.Application.Security;
using ServiceDesk.Domain.Entities;
using ServiceDesk.Infrastructure;
using ServiceDesk.Infrastructure.Persistence;
using ServiceDesk.Web.ErrorHandling;
using ServiceDesk.Web.Health;
using ServiceDesk.Web.Logging;
using ServiceDesk.Web.Middleware;
using ServiceDesk.Web.Security;

// ----- Fatal error hooks -----
// A small console and file logger for failures that happen before the host's own logger exists
// or outside any request. The app's main logger is created by the host further down.
Log.Logger = LoggingSetup.CreateFatalLogger();
AppDomain.CurrentDomain.UnhandledException += (_, e) =>
    Log.Fatal(e.ExceptionObject as Exception, "Unhandled exception. The process is shutting down.");
TaskScheduler.UnobservedTaskException += (_, e) =>
{
    Log.Error(e.Exception, "A background task failed and nothing observed the error.");
    e.SetObserved();
};

try
{
    // ----- Builder -----
    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

    // ----- Logging -----
    builder.Services.AddServiceDeskLogging();

    // ----- Application and infrastructure -----
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUser, CurrentUser>();

    builder.Services
        .AddIdentity<User, Role>(options =>
        {
            options.Password.RequiredLength = 10;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.AllowedForNewUsers = true;
            options.User.RequireUniqueEmail = false;
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders()
        .AddClaimsPrincipalFactory<PermissionClaimsPrincipalFactory>();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/account/login";
        options.LogoutPath = "/account/logout";
        options.AccessDeniedPath = "/account/access-denied";
        options.Cookie.Name = "sd.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            if (ApiRequest.IsApi(context.Request.HttpContext))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (ApiRequest.IsApi(context.Request.HttpContext))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

    builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    {
        options.ValidationInterval = TimeSpan.FromMinutes(5);
    });

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        foreach (var permission in Permissions.All)
        {
            options.AddPolicy(permission, policy => policy.RequireClaim(Permissions.ClaimType, permission));
        }
    });

    // ----- Web services -----
    // Write non-ASCII text (ñ in names, dashes) as-is instead of as &#x..; entities.
    // HTML-sensitive characters such as < > & " are still escaped.
    builder.Services.Configure<WebEncoderOptions>(options =>
        options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));
    builder.Services.AddRouting(options => options.LowercaseUrls = true);
    builder.Services.AddControllersWithViews(options =>
    {
        // Every POST, PUT, PATCH and DELETE must carry a valid antiforgery token.
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    });
    builder.Services.AddAntiforgery(options =>
    {
        options.HeaderName = "RequestVerificationToken";
        options.Cookie.Name = "sd.af";
        options.Cookie.SameSite = SameSiteMode.Strict;
        // SecurityHeadersMiddleware sends X-Frame-Options: DENY for every response.
        options.SuppressXFrameOptionsHeader = true;
    });
    builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier);
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("The web app is running."));

    var app = builder.Build();

    // ----- Pipeline -----
    app.UseMiddleware<SecurityHeadersMiddleware>();

    // Placed before the error pages, so each request is logged once, with its final status.
    app.UseSerilogRequestLogging(options =>
    {
        // The host owns the logger (the static one only records startup failures), so pass it in.
        options.Logger = app.Services.GetRequiredService<Serilog.ILogger>();
        options.GetLevel = RequestLogLevel.For;
    });

    if (!app.Environment.IsDevelopment())
    {
        // Development keeps the detailed developer exception page instead.
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }

    // Pages get a friendly error page; /api answers with problem details (JSON).
    app.UseWhen(context => !ApiRequest.IsApi(context), page => page.UseStatusCodePagesWithReExecute("/error/{0}"));
    app.UseWhen(ApiRequest.IsApi, api => api.UseStatusCodePages());

    app.UseHttpsRedirection();

    var culture = new CultureInfo("en-PH");
    app.UseRequestLocalization(new RequestLocalizationOptions
    {
        DefaultRequestCulture = new RequestCulture(culture),
        SupportedCultures = [culture],
        SupportedUICultures = [culture],
    });

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<ForcePasswordChangeMiddleware>();

    // ----- Endpoints -----
    app.MapStaticAssets().AllowAnonymous();
    app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = HealthResponseWriter.WriteAsync }).AllowAnonymous();
    app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    await IdentitySeeder.SeedAsync(app.Services);

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // HostAbortedException is how test hosts stop the app after building it; it isn't a failure.
    Log.Fatal(ex, "ServiceDesk failed to start.");
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>The app's entry point. Public so integration tests can host the app in memory.</summary>
public partial class Program;
