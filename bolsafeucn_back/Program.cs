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
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers; // <<-- para CORS (HeaderNames)
using Resend;
using Serilog;
using bolsafeucn_back.src.Infrastructure.Extensions;
using Microsoft.Extensions.FileProviders;

// Detectar entorno
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var isDevelopment = environment == "Development";

Console.WriteLine($"[INIT] Detectado entorno: {environment}");

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(
        new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build()
    )
    .CreateLogger();

Console.WriteLine("[INIT] Logger de Serilog configurado");

var builder = WebApplication.CreateBuilder(args);

// En producción, agregar variables de entorno con prioridad sobre appsettings.json
if (!isDevelopment)
{
    Console.WriteLine("[INIT] Agregando variables de entorno (modo producción)");
    builder.Configuration.AddEnvironmentVariables();
}

try
{
    Console.WriteLine("=================================================================");
    Console.WriteLine($"[INIT] 🚀 Iniciando aplicación en entorno: {environment}");
    Console.WriteLine("=================================================================");

    // Serilog
    Console.WriteLine("[STARTUP] Configurando Serilog como logger principal...");
    builder.Host.UseSerilog(
        (context, configuration) => configuration.ReadFrom.Configuration(context.Configuration)
    );
    Console.WriteLine("[STARTUP] ✓ Serilog configurado");

    Console.WriteLine("[STARTUP] Agregando servicios básicos (Controllers, Swagger)...");
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    Console.WriteLine("[STARTUP] ✓ Servicios básicos agregados");

    #region Identity
    // =========================
    // 1) Identity
    // =========================
    Console.WriteLine("[STARTUP] Configurando Identity...");
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
    Console.WriteLine("[STARTUP] Identity configurado correctamente");

    #endregion

    #region Auth
    // =========================
    // 2) Auth (JWT)
    // =========================
    Console.WriteLine("[STARTUP] Configurando autenticación JWT...");
    builder
        .Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            Console.WriteLine("[STARTUP] Leyendo configuración JWT...");
            string? jwtSecret = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                Console.WriteLine("[STARTUP] ❌ ERROR: La clave secreta JWT no está configurada");
                throw new InvalidOperationException("La clave secreta JWT no está configurada.");
            }
            Console.WriteLine("[STARTUP] ✓ Clave JWT encontrada");

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
    Console.WriteLine("[STARTUP] Autenticación JWT configurada correctamente");

    #endregion
    #region CORS
    // =========================
    // 3) CORS (permitimos el front en 3000)
    // =========================
    Console.WriteLine("[STARTUP] Configurando CORS...");
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
    Console.WriteLine("[STARTUP] CORS configurado correctamente");
    #endregion

    #region Resend
    // =========================
    // 4) Resend (emails)
    // =========================
    Console.WriteLine("[STARTUP] Configurando Resend (servicio de emails)...");
    builder.Services.AddOptions();
    builder.Services.AddHttpClient<ResendClient>();
    builder.Services.Configure<ResendClientOptions>(o =>
    {
        var apiKey = builder.Configuration.GetValue<string>("ResendApiKey");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("[STARTUP] ⚠️ ResendApiKey no configurada - emails no funcionarán");
        }
        else
        {
            Console.WriteLine("[STARTUP] ✓ ResendApiKey encontrada");
        }
        o.ApiToken = apiKey!;
    });
    builder.Services.AddTransient<IResend, ResendClient>();
    Console.WriteLine("[STARTUP] ✓ Resend configurado");

    #endregion
    #region PostgreSQL
    // =========================
    // 5) PostgreSQL
    // =========================
    Console.WriteLine("[STARTUP] Configurando conexión a PostgreSQL...");
    // Priorizar DATABASE_URL (estándar de Render) si existe
    var databaseUrl = builder.Configuration["DATABASE_URL"];
    var connectionString = !string.IsNullOrEmpty(databaseUrl)
        ? ConvertDatabaseUrlToConnectionString(databaseUrl)
        : builder.Configuration.GetConnectionString("DefaultConnection");

    if (!string.IsNullOrEmpty(databaseUrl))
    {
        Console.WriteLine("[STARTUP] Usando DATABASE_URL de variable de entorno");
    }
    else
    {
        Console.WriteLine("[STARTUP] Usando ConnectionString de appsettings.json");
    }
    Console.WriteLine("[STARTUP] Connection string configurado (host oculto por seguridad)");

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString)
    );
    Console.WriteLine("[STARTUP] PostgreSQL configurado correctamente con connection string");
    #endregion

    #region Hangfire
    // Hangfire - usa MemoryStorage por simplicidad
    Console.WriteLine("[STARTUP] Configurando Hangfire para tareas en segundo plano...");
    builder.Services.AddHangfire(configuration =>
        configuration.UseMemoryStorage()
    );
    builder.Services.AddHangfireServer();
    Console.WriteLine("[STARTUP] Hangfire configurado correctamente");
    #endregion


    #region DI
    // =========================
    // 6) DI (repos/services/mappers)
    // =========================
    Console.WriteLine("[STARTUP] Registrando servicios de Dependency Injection...");
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
    builder.Services.AddScoped<IAdminNotificationRepository, AdminNotificationRepository>();
    builder.Services.AddScoped<IFileRepository, FileRepository>();
    builder.Services.AddScoped<IPublicationRepository, PublicationRepository>();
    builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IOfferService, OfferService>();
    builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();
    builder.Services.AddScoped<IPublicationService, PublicationService>();
    builder.Services.AddScoped<IBuySellService, BuySellService>();
    builder.Services.AddScoped<IReviewService, ReviewService>();
    builder.Services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
    builder.Services.AddScoped<IFileService, FileService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddDocumentStorageProvider(builder.Configuration);


    builder.Services.AddMapster();
    Console.WriteLine("[STARTUP] Todos los servicios DI registrados correctamente");

    Console.WriteLine("[STARTUP] Construyendo aplicación...");
    var app = builder.Build();
    Console.WriteLine("[STARTUP] Aplicación construida exitosamente");

    #endregion
    #region Pipeline
    // =========================
    // Pipeline
    // =========================
    #endregion
    #region Hangfire Dashboard + Recurring Jobs
    // Hangfire dashboard (solo en desarrollo)
    Console.WriteLine("[STARTUP] Verificando entorno para Hangfire Dashboard...");
    if (app.Environment.IsDevelopment())
    {
        app.UseHangfireDashboard();
        // Registrar job recurrente cada hora para cerrar reviews vencidas
        RecurringJob.AddOrUpdate<IReviewService>(
            "CloseExpiredReviews",
            service => service.CloseExpiredReviewsAsync(),
            Cron.Hourly
        );
        Console.WriteLine("Hangfire dashboard habilitado y job recurrente para cierre de reviews programado. Servidor en: http://localhost:5185/hangfire");
    }

    #endregion
    #region Middleware
    // Middleware global de errores (antes de todo)
    Console.WriteLine("[STARTUP] Configurando middleware de manejo de errores...");
    app.UseMiddleware<bolsafeucn_back.src.API.Middlewares.ErrorHandlingMiddleware.ErrorHandlingMiddleware>();
    Console.WriteLine("[STARTUP] Middleware de errores configurado");
    #endregion

    // Seed DB + Mapster (al inicio)
    Console.WriteLine("[STARTUP] Iniciando proceso de seed de base de datos y configuración de Mapster...");
    await SeedAndMapDatabase(app);
    Console.WriteLine("[STARTUP] Seed y Mapster completados exitosamente");

    Console.WriteLine("[STARTUP] Verificando configuración de Swagger...");
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        Console.WriteLine("[STARTUP] ✓ Swagger UI habilitado en modo desarrollo");
    }
    else
    {
        Console.WriteLine("[STARTUP] Swagger deshabilitado (entorno de producción)");
    }

    // Si te genera líos en local (http->https), puedes comentar mientras desarrollas:
    // app.UseHttpsRedirection();

    Console.WriteLine("[STARTUP] Configurando pipeline de middleware final (CORS, Auth, Controllers)...");
    // CORS debe ir ANTES de auth/authorization
    app.UseCors("Frontend");
    Console.WriteLine("[STARTUP] ✓ CORS middleware habilitado");

    // Muy importante: primero autenticación, luego autorización
    app.UseAuthentication();
    Console.WriteLine("[STARTUP] ✓ Authentication middleware habilitado");
    app.UseAuthorization();
    Console.WriteLine("[STARTUP] ✓ Authorization middleware habilitado");

    app.MapControllers();
    Console.WriteLine("[STARTUP] ✓ Controllers mapeados correctamente");

    Console.WriteLine("[STARTUP] Aplicación iniciada correctamente - Todas las configuraciones completadas");
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        Console.WriteLine("🔥 SERVIDOR ASP.NET ARRANCÓ CORRECTAMENTE 🔥");
        Console.WriteLine("[STARTUP] ✅ SERVIDOR ARRANCÓ Y ESTÁ ESCUCHANDO PETICIONES");
    });

    Console.WriteLine("[STARTUP] Iniciando app.Run()...");
    app.Run();
    Console.WriteLine("[STARTUP] app.Run() terminó (esto solo se ve si el servidor se detiene)");
}
catch (Exception ex)
{
    Console.WriteLine("=================================================================");
    Console.WriteLine($"[ERROR] ❌ Aplicación terminó inesperadamente: {ex.Message}");
    Console.WriteLine($"[ERROR] Tipo de excepción: {ex.GetType().Name}");
    Console.WriteLine($"[ERROR] Mensaje: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"[ERROR] Inner Exception: {ex.InnerException.Message}");
    }
    Console.WriteLine($"[ERROR] Stack Trace: {ex.StackTrace}");
    Console.WriteLine("=================================================================");
    Console.WriteLine($"❌ ERROR FATAL: {ex.Message}");
    throw; // Re-lanzar para que el proceso termine con código de error
}
finally
{
    Console.WriteLine("[SHUTDOWN] Cerrando logger...");
    Log.CloseAndFlush();
    Console.WriteLine("[SHUTDOWN] Aplicación cerrada");
}

// =========================
// Helpers
// =========================
async Task SeedAndMapDatabase(IHost app)
{
    try
    {
        Console.WriteLine("[SEED] Creando scope de servicios...");
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var configuration = app.Services.GetRequiredService<IConfiguration>();

        Console.WriteLine("[SEED] Iniciando DataSeeder.Initialize()...");
        await DataSeeder.Initialize(configuration, serviceProvider);
        Console.WriteLine("[SEED] ✓ DataSeeder.Initialize() completado");

        Console.WriteLine("[SEED] Configurando Mapster...");
        MapperExtensions.ConfigureMapster(serviceProvider);
        Console.WriteLine("[SEED] ✓ Mapster configurado correctamente");

        Console.WriteLine("[SEED] ✓ Seed y configuración de mappers completados exitosamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SEED] ❌ Error crítico durante seed de base de datos: {ex.Message}");
        Console.WriteLine($"[SEED] Stack trace: {ex.StackTrace}");
        throw;
    }
}

static string ConvertDatabaseUrlToConnectionString(string databaseUrl)
{
    try
    {
        Console.WriteLine("[DB] Parseando DATABASE_URL...");
        Console.WriteLine($"[DB] DATABASE_URL recibida (parcial): {databaseUrl.Substring(0, Math.Min(30, databaseUrl.Length))}...");

        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');

        // Si uri.Port es -1, usar puerto por defecto 5432
        var port = uri.Port > 0 ? uri.Port : 5432;

        Console.WriteLine($"[DB] Host: {uri.Host}");
        Console.WriteLine($"[DB] Port: {port}");
        Console.WriteLine($"[DB] Database: {uri.AbsolutePath.TrimStart('/')}");
        Console.WriteLine($"[DB] Username: {userInfo[0]}");

        var connectionString = $"Server={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";

        Console.WriteLine("[DB] ✓ Connection string generado correctamente");
        return connectionString;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DB] ❌ Error al parsear DATABASE_URL: {ex.Message}");
        Console.WriteLine($"[DB] DATABASE_URL completa: {databaseUrl}");
        throw;
    }
}
