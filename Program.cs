using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Dignus.Data;
using Dignus.Data.Repositories;
using Dignus.Data.Repositories.Interfaces;
using Dignus.Candidate.Back.Services;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Candidate.Back.Mappings;
using Dignus.Candidate.Back.Configuration;
using Dignus.Candidate.Back.Authentication;
using Serilog;
using System.Reflection;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
// Configure JWT Authentication
var disableAuth = builder.Configuration.GetValue<bool>("DisableAuthentication", false);

if (!disableAuth)
{
    if (builder.Environment.IsProduction())
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
    }
    else
    {
        // Development mode - simple JWT configuration
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var secretKey = System.Text.Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not found"));

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(secretKey),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
    }
}
else
{
    // Authentication disabled for testing
    Log.Logger.Information("Authentication is DISABLED for development testing");
    builder.Services.AddAuthentication("Test")
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
}

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthentication", policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy("CandidateAccess", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("role", "candidate"));

    options.AddPolicy("RecruiterAccess", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("role", "recruiter"));
});

// Configure Entity Framework with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// New unified DbContext
builder.Services.AddDbContext<DignusContextNew>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    }));

// Configure AutoMapper (includes old and new profiles)
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AutoMapperProfile>(); // Old profile
    cfg.AddProfile<Dignus.Candidate.Back.Mapping.UnifiedProfile>(); // New unified profile
});

// Configure application settings
builder.Services.Configure<TestSettings>(builder.Configuration.GetSection(TestSettings.SectionName));
builder.Services.Configure<AzureStorageSettings>(builder.Configuration.GetSection("AzureStorage"));
builder.Services.Configure<MediaUploadSettings>(builder.Configuration.GetSection("MediaUploadSettings"));
builder.Services.Configure<AISettings>(builder.Configuration.GetSection("AISettings"));
builder.Services.Configure<ExternalAIAgentSettings>(builder.Configuration.GetSection("ExternalAIAgent"));
builder.Services.Configure<DatabricksSettings>(builder.Configuration.GetSection("Databricks"));
builder.Services.Configure<GupySettings>(builder.Configuration.GetSection(GupySettings.SectionName));
builder.Services.Configure<Dignus.Candidate.Back.Services.Auth.JwtSettings>(builder.Configuration.GetSection(Dignus.Candidate.Back.Services.Auth.JwtSettings.SectionName));
builder.Services.Configure<Dignus.Candidate.Back.Services.Email.EmailSettings>(builder.Configuration.GetSection(Dignus.Candidate.Back.Services.Email.EmailSettings.SectionName));
builder.Services.Configure<Dignus.Candidate.Back.Services.Consent.ConsentOptions>(builder.Configuration.GetSection(Dignus.Candidate.Back.Services.Consent.ConsentOptions.SectionName));

// Configure Repository and Unit of Work pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register new unified repositories
builder.Services.AddScoped<ITestInstanceRepository, TestInstanceRepository>();
builder.Services.AddScoped<IQuestionTemplateRepository, QuestionTemplateRepository>();
builder.Services.AddScoped<ITestQuestionSnapshotRepository, TestQuestionSnapshotRepository>();
builder.Services.AddScoped<ITestVideoResponseRepository, TestVideoResponseRepository>();
builder.Services.AddScoped<ITestQuestionResponseRepository, TestQuestionResponseRepository>();
builder.Services.AddScoped<IPortugueseReadingTextRepositoryNew, PortugueseReadingTextRepositoryNew>();
builder.Services.AddScoped<ITestQuestionGroupRepository, TestQuestionGroupRepository>();

// Register candidate repository
builder.Services.AddScoped<Dignus.Data.Repositories.Interfaces.ICandidateRepository, Dignus.Data.Repositories.CandidateRepository>();

// Register authentication repositories
builder.Services.AddScoped<Dignus.Data.Repositories.Interfaces.ICandidateAuthTokenRepository, Dignus.Data.Repositories.CandidateAuthTokenRepository>();

// Register consent repositories
builder.Services.AddScoped<Dignus.Data.Repositories.Interfaces.ICandidateConsentRepository, Dignus.Data.Repositories.CandidateConsentRepository>();

// Configure Azure Storage
builder.Services.AddSingleton(provider =>
{
    var storageSettings = builder.Configuration.GetSection("AzureStorage").Get<AzureStorageSettings>();
    return new BlobServiceClient(storageSettings?.ConnectionString);
});

// Configure Services
builder.Services.AddScoped<ICandidateService, CandidateService>();
builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddScoped<IEvaluationService, EvaluationService>();

// Unified schema services
builder.Services.AddScoped<IVideoResponseService, VideoResponseService>();
builder.Services.AddScoped<IQuestionResponseService, QuestionResponseService>();
builder.Services.AddScoped<IQuestionTemplateService, QuestionTemplateService>();

// Job service
builder.Services.AddScoped<IJobService, JobService>();

// Authentication services
builder.Services.AddScoped<Dignus.Candidate.Back.Services.Auth.IJwtTokenService, Dignus.Candidate.Back.Services.Auth.JwtTokenService>();
builder.Services.AddScoped<Dignus.Candidate.Back.Services.Auth.ICandidateAuthenticationService, Dignus.Candidate.Back.Services.Auth.CandidateAuthenticationService>();

// External integration services
builder.Services.AddScoped<Dignus.Candidate.Back.Services.External.IGupyIntegrationService, Dignus.Candidate.Back.Services.External.GupyIntegrationService>();

// Email service
builder.Services.AddScoped<Dignus.Candidate.Back.Services.Email.IEmailService, Dignus.Candidate.Back.Services.Email.EmailService>();

// Consent service
builder.Services.AddScoped<Dignus.Candidate.Back.Services.Consent.IConsentService, Dignus.Candidate.Back.Services.Consent.ConsentService>();

// Admin services
builder.Services.AddScoped<Dignus.Candidate.Back.Services.Admin.IQuestionGroupAdminService, Dignus.Candidate.Back.Services.Admin.QuestionGroupAdminService>();

// TimeProvider for testability
builder.Services.AddSingleton(TimeProvider.System);

// Configure Evaluation and Reporting Services
builder.Services.AddScoped<IEvaluationEngineService, EvaluationEngineService>();
builder.Services.AddScoped<IReportGenerationService, ReportGenerationService>();
builder.Services.AddScoped<IBenchmarkService, BenchmarkService>();

// Configure Databricks Integration Service
builder.Services.AddScoped<IDatabricksIntegrationService, DatabricksIntegrationService>();

// Configure LangChain and Google AI services
// GoogleProvider removed - AI analysis handled by separate application

// Configure External AI Agent Service
builder.Services.AddHttpClient<IExternalAIAgentService, ExternalAIAgentService>(client =>
{
    var settings = builder.Configuration.GetSection("ExternalAIAgent").Get<ExternalAIAgentSettings>();
    if (!string.IsNullOrWhiteSpace(settings?.BaseUrl))
    {
        client.BaseAddress = new Uri(settings.BaseUrl);
    }
    client.Timeout = TimeSpan.FromSeconds(settings?.TimeoutSeconds ?? 30);
}).AddStandardResilienceHandler();

builder.Services.AddHttpClient<IDatabricksIntegrationService, DatabricksIntegrationService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(30);
}).AddStandardResilienceHandler();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<DignusContextNew>()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DignusPolicy", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add controllers and API services
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.ProducesAttribute("application/json"));
})
.AddJsonOptions(options =>
{
    // Workaround for .NET 9 TestServer compatibility issue
    // The TestServer's ResponseBodyPipeWriter doesn't implement UnflushedBytes
    // Setting DefaultBufferSize to 1 (minimum value) forces smaller buffer writes which avoids the issue
    var isTestMode = builder.Configuration.GetValue<bool>("SkipDatabaseInitialization", false);
    if (isTestMode) // Only apply workaround in test mode
    {
        options.JsonSerializerOptions.DefaultBufferSize = 1;
    }
});

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Dignus Candidate API",
        Version = "v1.0",
        Description = "REST API for the Dignus Candidate system to manage the candidate selection process, including test submissions, AI analysis, and recruiter integration.",
        Contact = new OpenApiContact
        {
            Name = "Dignus Development Team"
        }
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

    // Include XML comments if available
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Build the application
var app = builder.Build();

// Ensure database is created and migrations are applied (skip for tests using InMemory)
var skipDbInit = builder.Configuration.GetValue<bool>("SkipDatabaseInitialization", false);
if (!skipDbInit)
{
    await EnsureDatabaseAsync(app);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Dignus Candidate API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Dignus Candidate API Documentation";
        options.DefaultModelsExpandDepth(-1);
    });
}

// Security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    
    await next();
});

// app.UseHttpsRedirection();

app.UseCors("DignusPolicy");

// Add request logging middleware
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? Serilog.Events.LogEventLevel.Error
        : httpContext.Response.StatusCode > 499
            ? Serilog.Events.LogEventLevel.Error
            : Serilog.Events.LogEventLevel.Information;
});

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map health checks endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    AllowCachingResponses = false,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration
            })
        });
        await context.Response.WriteAsync(result);
    }
}); // .RequireAuthorization("RequireAuthentication"); // TEMPORARILY DISABLED

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    AllowCachingResponses = false,
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    AllowCachingResponses = false,
    Predicate = _ => false // Exclude all checks for basic liveness probe
});

app.MapControllers();

app.Run();

/// <summary>
/// Ensures the database exists and applies any pending migrations
/// </summary>
/// <param name="app">The web application instance</param>
static async Task EnsureDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Handle DbContext
    try
    {
        logger.LogInformation("Checking unified database context (DignusContextNew)...");
        var context = scope.ServiceProvider.GetRequiredService<DignusContextNew>();
        await ApplyMigrationsAsync(context, logger, "DignusContextNew");
        logger.LogInformation("Unified database context (DignusContextNew) is ready.");

        // Seed initial question groups
        await SeedQuestionGroupsAsync(context, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while ensuring database readiness for DignusContextNew.");

        // In production, you might want to fail fast
        if (app.Environment.IsProduction())
        {
            logger.LogCritical("Database initialization failed in production environment. Application will not start.");
            throw;
        }

        // In development, log the error but continue (for development scenarios)
        logger.LogWarning("Database initialization failed in development environment. Continuing startup...");
    }
}

static async Task ApplyMigrationsAsync(DbContext context, ILogger<Program> logger, string contextName)
{
    logger.LogInformation("Checking database connection and migrations for {ContextName}...", contextName);

    // Check if database can be connected to
    var canConnect = await context.Database.CanConnectAsync();
    if (!canConnect)
    {
        logger.LogInformation("Database does not exist. Creating database for {ContextName}...", contextName);
        await context.Database.EnsureCreatedAsync();
        logger.LogInformation("Database created successfully for {ContextName}.", contextName);
    }

    // Check for pending migrations
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        logger.LogInformation("Found {MigrationCount} pending migrations for {ContextName}. Applying migrations...",
            pendingMigrations.Count(), contextName);
        foreach (var migration in pendingMigrations)
        {
            logger.LogInformation("Pending migration for {ContextName}: {Migration}", contextName, migration);
        }

        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully for {ContextName}.", contextName);
    }
    else
    {
        logger.LogInformation("Database is up to date for {ContextName}. No migrations needed.", contextName);
    }

    // Verify the database schema
    var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
    logger.LogInformation("Database is ready for {ContextName} with {MigrationCount} applied migrations.",
        contextName, appliedMigrations.Count());
}

static async Task SeedQuestionGroupsAsync(DignusContextNew context, ILogger<Program> logger)
{
    try
    {
        logger.LogInformation("Running question group seeder...");

        // Create logger for the seeder using LoggerFactory
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var seederLogger = loggerFactory.CreateLogger<Dignus.Data.Seeders.QuestionGroupSeeder>();

        var seeder = new Dignus.Data.Seeders.QuestionGroupSeeder(context, seederLogger);
        await seeder.SeedAsync();
        logger.LogInformation("Question group seeding completed.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred while seeding question groups");
        // Don't throw - seeding failure shouldn't prevent app startup
    }
}


// Make Program class accessible for testing
public partial class Program { }
