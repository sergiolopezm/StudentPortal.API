using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentPortal.API.Attributes;
using StudentPortal.API.Domain.Contracts;
using StudentPortal.API.Domain.Contracts.EstudianteRepository;
using StudentPortal.API.Domain.Contracts.InscripcionEstudianteRepository;
using StudentPortal.API.Domain.Contracts.MateriaRepository;
using StudentPortal.API.Domain.Contracts.ProfesorRepository;
using StudentPortal.API.Domain.Contracts.ProgramaRepository;
using StudentPortal.API.Domain.Services;
using StudentPortal.API.Domain.Services.EstudianteService;
using StudentPortal.API.Domain.Services.InscripcionEstudianteService;
using StudentPortal.API.Domain.Services.MateriaService;
using StudentPortal.API.Domain.Services.ProfesorService;
using StudentPortal.API.Domain.Services.ProgramaService;
using StudentPortal.API.Extensions;
using StudentPortal.API.Infrastructure;
using StudentPortal.API.Util.Extensions;
using StudentPortal.API.Util.Logging;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger usando las extensiones
builder.Services.AddCustomSwagger();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy => policy
            .WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:5000",
                "https://localhost:5001")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Configurar DbContext
builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar autenticación JWT
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key no configurada"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = !string.IsNullOrEmpty(jwtSection["Issuer"]),
        ValidIssuer = jwtSection["Issuer"],
        ValidateAudience = !string.IsNullOrEmpty(jwtSection["Audience"]),
        ValidAudience = jwtSection["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Registrar el sistema de logging de archivos
builder.Services.AddScoped<IFileLogger, FileLoggerService>();
builder.Services.AddScoped<StudentPortal.API.Domain.Contracts.ILoggerFactory, StudentPortal.API.Domain.Services.LoggerFactory>();

// Registrar filtros y atributos personalizados
builder.Services.AddScoped<AccesoAttribute>();
builder.Services.AddScoped<ExceptionAttribute>();
builder.Services.AddScoped<LogAttribute>();
builder.Services.AddScoped<ValidarModeloAttribute>();
builder.Services.AddScoped<JwtAuthorizationAttribute>();

// Registrar repositorios existentes
builder.Services.AddScoped<IAccesoRepository, AccesoRepository>();
builder.Services.AddScoped<ILogRepository, LogRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Registrar nuevos repositorios
builder.Services.AddScoped<IProgramaRepository, ProgramaRepository>();
builder.Services.AddScoped<IMateriaRepository, MateriaRepository>();
builder.Services.AddScoped<IProfesorRepository, ProfesorRepository>();
builder.Services.AddScoped<IEstudianteRepository, EstudianteRepository>();
builder.Services.AddScoped<IInscripcionEstudianteRepository, InscripcionEstudianteRepository>();

// Configurar filtros globales
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExceptionAttribute>();
});

var app = builder.Build();

// Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseCustomSwagger();
}
else
{
    app.Use(async (context, next) =>
    {
        var errorHandler = new ErrorHandlingMiddleware(
            next,
            context.RequestServices.GetRequiredService<ILogger<ErrorHandlingMiddleware>>(),
            context.RequestServices.GetRequiredService<StudentPortal.API.Domain.Contracts.ILoggerFactory>()
        );
        await errorHandler.Invoke(context);
    });
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCustomCors();

app.UseRouting();

// Middleware de logging
app.Use(async (context, next) =>
{
    var loggingHandler = new LoggingMiddleware(
        next,
        context.RequestServices.GetRequiredService<ILogger<LoggingMiddleware>>(),
        context.RequestServices.GetRequiredService<StudentPortal.API.Domain.Contracts.ILoggerFactory>()
    );
    await loggingHandler.Invoke(context);
});

// Middleware de autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Usar endpoints personalizados
app.UseCustomEndpoints();

// Crear la base de datos y aplicar migraciones en desarrollo
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

    // Asegurarse de que la base de datos esté creada
    if (dbContext.Database.EnsureCreated())
    {
        Console.WriteLine("Base de datos creada correctamente");
    }

    // Aplicar migraciones pendientes (si hay alguna)
    var pendingMigrations = dbContext.Database.GetPendingMigrations();
    if (pendingMigrations.Any())
    {
        Console.WriteLine("Aplicando migraciones pendientes...");
        dbContext.Database.Migrate();
        Console.WriteLine("Migraciones aplicadas correctamente");
    }
}

app.Run();