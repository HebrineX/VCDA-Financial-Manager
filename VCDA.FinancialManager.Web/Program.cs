using System.Security.Claims;
using System.Threading.RateLimiting;
using System.Text;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Cryptography.X509Certificates;
using VCDA.FinancialManager.Web.Components;
using VCDA.FinancialManager.Web.Components.Account;
using VCDA.FinancialManager.Web.Data;
using VCDA.FinancialManager.Web.Models;
using VCDA.FinancialManager.Web.Services;

var builder = WebApplication.CreateBuilder(args);
var enforceHttps = builder.Configuration.GetValue("App:EnforceHttps", !builder.Environment.IsDevelopment());
var resetDatabaseOnStartup = builder.Configuration.GetValue("App:ResetDatabaseOnStartup", false);

builder.Services.AddScoped<FinancialService>();
builder.Services.AddScoped<AdminUserService>();
builder.Services.AddScoped<PublicLinkService>();
builder.Services.AddOptions<SmtpOptions>()
    .Bind(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(AppRoles.Admin));
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 120,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
});

// Configurar Serilog primero para poder usarlo en el resto del arranque
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Configurar DataProtection con certificado X.509 para cifrar claves en reposo
var certPath = Environment.GetEnvironmentVariable("DATAPROTECTION_CERT_PATH");
var certPassword = Environment.GetEnvironmentVariable("DATAPROTECTION_CERT_PASSWORD");

var dpBuilder = builder.Services.AddDataProtection()
    .SetApplicationName("VCDA-Financial-Manager")
    .PersistKeysToFileSystem(new DirectoryInfo("/home/app/.aspnet/DataProtection-Keys"));

if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath) && !string.IsNullOrEmpty(certPassword))
{
    var certBytes = File.ReadAllBytes(certPath);
    var cert = X509CertificateLoader.LoadPkcs12(certBytes, certPassword);
    dpBuilder.ProtectKeysWithCertificate(cert);
    Log.Information("DataProtection: claves cifradas con certificado X.509 desde {CertPath}", certPath);
}
else
{
    Log.Warning("DataProtection: certificado no encontrado en {CertPath}. Las claves no estarán cifradas en reposo.", certPath ?? "N/A");
}


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.User.RequireUniqueEmail = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddTransient<IEmailSender<ApplicationUser>, SmtpEmailSender>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    if (resetDatabaseOnStartup)
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureDeletedAsync();
        Log.Warning("Base de datos reiniciada por App:ResetDatabaseOnStartup=true.");
    }

    await IdentitySeed.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();
app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
    context.Response.Headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});
app.UseRateLimiter();
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
if (enforceHttps)
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapGet("/api/reportes/exportar-csv", async (
    HttpContext httpContext,
    FinancialService financialService,
    DateTime? desde,
    DateTime? hasta,
    Guid? categoriaId,
    Guid? cuentaId) =>
{
    if (httpContext.User.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var filter = new ReportFilter
    {
        Desde = desde,
        Hasta = hasta,
        CategoriaId = categoriaId,
        CuentaId = cuentaId,
        Page = 1,
        PageSize = int.MaxValue
    };

    var transacciones = await financialService.GetTransaccionesForExportAsync(userId, filter);
    var csv = CsvImportParser.BuildExportCsv(transacciones);
    var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();
    return Results.File(bytes, "text/csv", $"reporte-{DateTime.UtcNow:yyyyMMdd}.csv");
}).RequireAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok", app = "VCDA-Financial-Manager" }));

app.Run();
