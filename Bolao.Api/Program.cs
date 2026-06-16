using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using Bolao.Api.Authentication;
using Bolao.Api.Authorization;
using Bolao.Api.BackgroundServices;
using Bolao.Api.HealthChecks;
using Bolao.Api.Middlewares;
using Bolao.Api.Services;
using Bolao.Api.Validators.Boloes;
using Bolao.Application.Boloes.CreateBolao;
using Bolao.Application.Boloes.DeleteBolao;
using Bolao.Application.Boloes.GetBolaoById;
using Bolao.Application.Boloes.GetBolaoCurrentRoundMatches;
using Bolao.Application.Boloes.GetBolaoDashboard;
using Bolao.Application.Boloes.GetBolaoMatches;
using Bolao.Application.Boloes.GetBolaoMembers;
using Bolao.Application.Boloes.GetBolaoRanking;
using Bolao.Application.Boloes.GetMemberFinishedPredictions;
using Bolao.Application.Boloes.GetMyPools;
using Bolao.Application.Boloes.JoinBolao;
using Bolao.Application.Boloes.LeaveBolao;
using Bolao.Application.Boloes.RemoveBolaoMember;
using Bolao.Application.Common.Interfaces;
using Bolao.Application.Home.GetHome;
using Bolao.Application.Matches.GetMatchDetails;
using Bolao.Application.Predictions.CalculatePredictionPoints;
using Bolao.Application.Predictions.CreatePrediction;
using Bolao.Application.Predictions.GetMyPredictions;
using Bolao.Application.Predictions.GetPredictionHistory;
using Bolao.Application.Predictions.ProcessFinishedMatches;
using Bolao.Application.WorldCup.GetWorldCupMatches;
using Bolao.Application.WorldCup.SyncWorldCup;
using Bolao.Infrastructure.ExternalServices.ApiFutebol;
using Bolao.Infrastructure.Persistence;
using Bolao.Infrastructure.Services;
using FirebaseAdmin;
using FluentValidation;
using FluentValidation.AspNetCore;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();
    });

    var firebaseCredentialsJson =
        builder.Configuration["Firebase:CredentialsJson"];

    if (!string.IsNullOrWhiteSpace(firebaseCredentialsJson))
    {
        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromJson(firebaseCredentialsJson)
        });
    }
    else
    {
        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile("Firebase/firebase-adminsdk.json")
        });
    }

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<IApplicationDbContext>(provider =>
        provider.GetRequiredService<AppDbContext>());

    builder.Services.Configure<ApiFutebolSettings>(
        builder.Configuration.GetSection("ApiFutebol"));

    builder.Services.AddHttpClient<IApiFutebolService, ApiFutebolService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<IInviteCodeGenerator, InviteCodeGenerator>();
    builder.Services.AddScoped<ICacheService, CacheService>();

    builder.Services.AddHttpClient<ITeamLogoService, TeamLogoService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
    });

    builder.Services.AddScoped<CreateBolaoUseCase>();
    builder.Services.AddScoped<JoinBolaoUseCase>();
    builder.Services.AddScoped<GetMyPoolsUseCase>();
    builder.Services.AddScoped<GetBolaoByIdUseCase>();
    builder.Services.AddScoped<GetBolaoMatchesUseCase>();
    builder.Services.AddScoped<GetBolaoCurrentRoundMatchesUseCase>();
    builder.Services.AddScoped<GetBolaoMembersUseCase>();
    builder.Services.AddScoped<GetBolaoRankingUseCase>();
    builder.Services.AddScoped<GetMemberFinishedPredictionsUseCase>();
    builder.Services.AddScoped<GetBolaoDashboardUseCase>();
    builder.Services.AddScoped<LeaveBolaoUseCase>();
    builder.Services.AddScoped<DeleteBolaoUseCase>();
    builder.Services.AddScoped<RemoveBolaoMemberUseCase>();

    builder.Services.AddScoped<CreatePredictionUseCase>();
    builder.Services.AddScoped<GetMyPredictionsUseCase>();
    builder.Services.AddScoped<GetPredictionHistoryUseCase>();
    builder.Services.AddScoped<ProcessFinishedMatchesUseCase>();
    builder.Services.AddScoped<CalculatePredictionPointsUseCase>();

    builder.Services.AddScoped<IWorldCupSyncService, WorldCupSyncService>();
    builder.Services.AddScoped<SyncWorldCupUseCase>();
    builder.Services.AddScoped<GetWorldCupMatchesUseCase>();

    builder.Services.AddScoped<IPredictionScoreCalculator, PredictionScoreCalculator>();

    builder.Services.AddScoped<GetHomeUseCase>();
    builder.Services.AddScoped<GetMatchDetailsUseCase>();

    builder.Services.AddSingleton<IAuthorizationHandler, AdminOnlyHandler>();

    builder.Services.AddHostedService<WorldCupBackgroundService>();

    builder.Services.AddMemoryCache();

    builder.Services
        .AddAuthentication("Firebase")
        .AddScheme<AuthenticationSchemeOptions, FirebaseAuthenticationHandler>(
            "Firebase",
            options => { });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.AddRequirements(new AdminOnlyRequirement());
        });
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.ContentType = "application/json";

            var response = new
            {
                message = "Muitas requisições. Tente novamente em alguns instantes.",
                statusCode = StatusCodes.Status429TooManyRequests,
                traceId = context.HttpContext.TraceIdentifier
            };

            await context.HttpContext.Response.WriteAsync(
                JsonSerializer.Serialize(response),
                cancellationToken);
        };

        options.AddPolicy("General", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: GetRateLimitPartitionKey(context),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

        options.AddPolicy("Sensitive", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: GetRateLimitPartitionKey(context),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

        options.AddPolicy("CreatePool", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: GetRateLimitPartitionKey(context),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
    });

    builder.Services
        .AddHealthChecks()
        .AddSqlServer(
            connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "sql-server",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "ready", "database" })
        .AddCheck<ApiFutebolHealthCheck>(
            name: "api-futebol-config",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "ready", "external" });

    builder.Services.AddControllers();

    builder.Services.AddFluentValidationAutoValidation(options =>
    {
        options.DisableDataAnnotationsValidation = true;
    });

    builder.Services.AddValidatorsFromAssemblyContaining<CreateBolaoRequestValidator>();

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Digite: Bearer {seu token Firebase}"
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

    app.UseSerilogRequestLogging();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseSwagger();
    app.UseSwaggerUI();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseAuthentication();

    app.UseRateLimiter();

    app.UseAuthorization();

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = WriteHealthCheckResponse
    });

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = WriteHealthCheckResponse
    });

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A API encerrou inesperadamente.");
}
finally
{
    Log.CloseAndFlush();
}

static string GetRateLimitPartitionKey(HttpContext context)
{
    var firebaseUid =
        context.User.FindFirstValue("firebase_uid")
        ?? context.User.FindFirstValue("uid")
        ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (!string.IsNullOrWhiteSpace(firebaseUid))
        return $"user:{firebaseUid}";

    var ip = context.Connection.RemoteIpAddress?.ToString();

    return string.IsNullOrWhiteSpace(ip)
        ? "anonymous"
        : $"ip:{ip}";
}

static async Task WriteHealthCheckResponse(
    HttpContext context,
    HealthReport report)
{
    context.Response.ContentType = "application/json";

    var response = new
    {
        status = report.Status.ToString(),
        totalDuration = report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString(),
            description = entry.Value.Description,
            duration = entry.Value.Duration.TotalMilliseconds
        })
    };

    await context.Response.WriteAsync(
        JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }));
}