using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using CPG.Api.Hubs;
using CPG.Api.Infrastructure;
using CPG.Application;
using CPG.Application.Common.Interfaces;
using CPG.Infrastructure;
using CPG.Infrastructure.Persistence;
using CPG.Infrastructure.Telemetry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

// --- Application + Infrastructure (Clean Architecture composition) ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// --- Web layer ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// --- Real-time telemetry (SignalR) ---
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton<ITelemetryBroadcaster, SignalRTelemetryBroadcaster>();
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHostedService<FleetTelemetrySimulator>();
}

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddCpgSwagger();
builder.Services.AddCpgObservability();

// --- Security: JWT bearer + RBAC policies (SPEC.md US-01) ---
// Bound lazily from JwtOptions so WebApplicationFactory config overrides are honoured.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<CPG.Infrastructure.Identity.JwtOptions>>((bearer, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;
        var signingKey = string.IsNullOrWhiteSpace(jwt.SigningKey)
            ? throw new InvalidOperationException("Jwt:SigningKey must be configured.")
            : jwt.SigningKey;

        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
        };

        // SignalR/WebSockets can't set Authorization headers — take the JWT from the
        // access_token query string on hub requests.
        bearer.Events = new JwtBearerEvents
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
            },
        };
    });

builder.Services.AddAuthorizationBuilder().AddCpgAuthorization();
builder.Services.AddCpgAuthorizationResultHandler();

// --- Health checks (connection string resolved lazily from configuration) ---
builder.Services.AddHealthChecks()
    .AddNpgSql(
        sp => sp.GetRequiredService<IConfiguration>().GetConnectionString("Postgres")
            ?? "Host=localhost;Port=5432;Database=cpg;Username=cpg;Password=cpg_local_dev",
        name: "postgres",
        tags: ["ready"]);

// --- CORS: origins come from Cors:AllowedOrigins (Azure Container Apps env) and fall back
//     to the local Vite dev/preview servers when nothing is configured. ---
const string CorsPolicyName = "cpg-frontend";
var corsSeparators = new[] { ',', ';' };
var corsSection = builder.Configuration.GetSection("Cors:AllowedOrigins");
var configuredOrigins = corsSection.GetChildren().Any()
    ? corsSection.Get<string[]>() ?? []
    : (corsSection.Value ?? string.Empty)
        .Split(corsSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
var corsOrigins = configuredOrigins.Length > 0
    ? configuredOrigins
    : ["http://localhost:5173", "http://localhost:4173"];

builder.Services.AddCors(options => options.AddPolicy(CorsPolicyName, policy =>
    policy.WithOrigins(corsOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

var app = builder.Build();

// Apply migrations + seed the RBAC baseline users outside Production (SPEC.md US-01).
if (!app.Environment.IsProduction())
{
    await app.Services.InitialiseDatabaseAsync();
}

// Warm the CQRS + validation pipeline so the first real rate request also meets the
// <500 ms budget (SPEC.md US-02) - pays JIT / validator-compilation cost up front.
await app.Services.WarmUpRateEngineAsync();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<IdempotencyKeyMiddleware>();

app.MapControllers();
app.MapHub<TelemetryHub>("/hubs/telemetry");
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
});

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

await app.RunAsync();

/// <summary>Exposed so <c>WebApplicationFactory&lt;Program&gt;</c> can host the API in integration tests.</summary>
public partial class Program;
