using bolsafe_ucn.src.Application.Services.Interfaces;
using bolsafeucn_back.src.Application.Infrastructure.Data;
using bolsafeucn_back.src.Application.Mappers;
using bolsafeucn_back.src.Application.Services.Implements;
using bolsafeucn_back.src.Application.Services.Interfaces;
using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Infrastructure.Data;
using bolsafeucn_back.src.Infrastructure.Repositories.Implements;
using bolsafeucn_back.src.Infrastructure.Repositories.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers; // <<-- para CORS (HeaderNames)
using Resend;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(
        new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build()
    )
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

try
{
    Log.Information("Starting web application");

    // Serilog
    builder.Host.UseSerilog(
        (context, configuration) => configuration.ReadFrom.Configuration(context.Configuration)
    );

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // =========================
    // 1) Identity
    // =========================
    builder
        .Services.AddIdentity<GeneralUser, Role>(options =>
        {
            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
        })
        .AddRoles<Role>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    // =========================
    // 2) Auth (JWT)
    // =========================
    builder
        .Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            string? jwtSecret = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("La clave secreta JWT no está configurada.");
            }

            options.TokenValidationParameters =
                new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(jwtSecret)
                    ),
                    ValidateLifetime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero,
                };
        });

    // =========================
    // 3) CORS (permitimos el front en 3000)
    // =========================
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(
            "Frontend",
            policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:3000" // Next.js dev
                    // ,"https://localhost:3000"  // agrega si usas https en front
                    // ,"https://localhost:7129"  // agrega si llamas al backend en https y navegas desde https
                    )
                    .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization, "Accept")
                    .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
                    .AllowCredentials(); // opcional si luego usas cookies
            }
        );
    });

    // =========================
    // 4) Resend (emails)
    // =========================
    builder.Services.AddOptions();
    builder.Services.AddHttpClient<ResendClient>();
    builder.Services.Configure<ResendClientOptions>(o =>
    {
        o.ApiToken = builder.Configuration.GetValue<string>("ResendApiKey")!;
    });
    builder.Services.AddTransient<IResend, ResendClient>();

    // =========================
    // 5) PostgreSQL
    // =========================
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

    // =========================
    // 6) DI (repos/services/mappers)
    // =========================
    builder.Services.AddScoped<StudentMapper>();
    builder.Services.AddScoped<IndividualMapper>();
    builder.Services.AddScoped<CompanyMapper>();
    builder.Services.AddScoped<AdminMapper>();
    builder.Services.AddScoped<OfferMapper>();
    builder.Services.AddScoped<ProfileMapper>();

    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IOfferRepository, OfferRepository>();
    builder.Services.AddScoped<IBuySellRepository, BuySellRepository>();
    builder.Services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
    builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
    builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IFileRepository, FileRepository>();

    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IOfferService, OfferService>();
    builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();
    builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
    builder.Services.AddScoped<IPublicationService, PublicationService>();
    builder.Services.AddScoped<IBuySellService, BuySellService>();
    builder.Services.AddScoped<IPublicationRepository, PublicationRepository>();
    builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
    builder.Services.AddScoped<IReviewService, ReviewService>();
    builder.Services.AddScoped<IFileService, FileService>();

    builder.Services.AddMapster();

    var app = builder.Build();

    // =========================
    // Pipeline
    // =========================

    // Middleware global de errores (antes de todo)
    app.UseMiddleware<bolsafeucn_back.src.API.Middlewares.ErrorHandlingMiddleware.ErrorHandlingMiddleware>();

    // Seed DB + Mapster (al inicio)
    await SeedAndMapDatabase(app);

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        Log.Information("Swagger UI habilitado en modo desarrollo");
    }

    // Si te genera líos en local (http->https), puedes comentar mientras desarrollas:
    // app.UseHttpsRedirection();

    // CORS debe ir ANTES de auth/authorization
    app.UseCors("Frontend");

    // Muy importante: primero autenticación, luego autorización
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Aplicación iniciada correctamente");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// =========================
// Helpers
// =========================
async Task SeedAndMapDatabase(IHost app)
{
    using var scope = app.Services.CreateScope();
    var serviceProvider = scope.ServiceProvider;
    var configuration = app.Services.GetRequiredService<IConfiguration>();

    Log.Information("Iniciando seed de base de datos y configuración de mappers");
    await DataSeeder.Initialize(configuration, serviceProvider);
    MapperExtensions.ConfigureMapster(serviceProvider);
    Log.Information("Seed de base de datos y configuración de mappers completados");
}
