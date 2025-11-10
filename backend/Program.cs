using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Data;
using NFLFantasyAPI.Services;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Backend.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/nfl-fantasy-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuración de Swagger con soporte para file uploads
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "NFL Fantasy API", 
        Version = "v1",
        Description = "API para gestión de jugadores y equipos NFL Fantasy"
    });

    // Configuración para soportar file uploads
    c.OperationFilter<FileUploadOperationFilter>();
});

// Configurar DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar el servicio de batch
builder.Services.AddScoped<JugadorBatchService>();

// Configurar FileServerSettings
builder.Services.Configure<FileServerSettings>(
    builder.Configuration.GetSection("FileServer"));

// ==========================================
// CONFIGURACIÓN JWT - CORREGIDA
// ==========================================
// 1. Registrar JwtSettings en el contenedor de dependencias
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// 2. Leer configuración para autenticación
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

if (!string.IsNullOrEmpty(secretKey))
{
    Log.Information("Configurando autenticación JWT...");
    
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();
    Log.Information("Autenticación JWT configurada correctamente");
}
else
{
    Log.Warning("JWT Secret no configurado. Autenticación deshabilitada.");
}
// ==========================================

// Configurar CORS
const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "http://localhost:3000", "http://192.168.100.77:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NFL Fantasy API v1");
        c.RoutePrefix = "swagger";
    });
}

// Configurar middleware de manejo de excepciones global
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature =
            context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

        var exception = exceptionHandlerPathFeature?.Error;

        Log.Error(exception, "Error no controlado en la aplicación");

        await context.Response.WriteAsJsonAsync(new
        {
            error = "Error interno del servidor",
            message = app.Environment.IsDevelopment() ? exception?.Message : null
        });
    });
});

// Crear estructura de carpetas necesarias
var uploadsPath = Path.Combine(app.Environment.WebRootPath, "uploads", "equipos");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
    Log.Information("Carpeta de uploads creada: {UploadsPath}", uploadsPath);
}

var processedJugadoresPath = Path.Combine(app.Environment.WebRootPath, "processed", "jugadores");
if (!Directory.Exists(processedJugadoresPath))
{
    Directory.CreateDirectory(processedJugadoresPath);
    Log.Information("Carpeta de archivos procesados (jugadores) creada: {ProcessedPath}", processedJugadoresPath);
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors(myAllowSpecificOrigins);

// Agregar middleware de autenticación y autorización
if (!string.IsNullOrEmpty(secretKey))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapControllers();

Log.Information("🚀 NFL Fantasy API iniciada correctamente");
Log.Information("📖 Swagger disponible en: http://localhost:5000/swagger");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}

// ==========================================
// FILTRO PARA FILE UPLOADS EN SWAGGER
// ==========================================
public class FileUploadOperationFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
    {
        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile))
            .ToList();

        if (!fileParams.Any())
            return;

        // Limpiar parámetros existentes que causan conflicto
        operation.Parameters?.Clear();

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = fileParams.ToDictionary(
                            p => p.Name ?? "file",
                            p => new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary",
                                Description = "Archivo JSON con array de jugadores"
                            }
                        ),
                        Required = new HashSet<string>(fileParams.Select(p => p.Name ?? "file"))
                    }
                }
            }
        };
    }
}