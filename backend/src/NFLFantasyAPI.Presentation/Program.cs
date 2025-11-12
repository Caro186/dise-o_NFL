using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Logic.Service;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NFLFantasyAPI.Logic.DbContextProvider;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Logic.Services;
using NFLFantasyAPI.CrossCutting.Interface;
using NFLFantasyAPI.CrossCutting.Configuration;
using System.Text;


namespace NFLFantasyAPI.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configurar JwtSettings
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

            // Configurar FileServer settings
            builder.Services.Configure<FileServerSettings>(builder.Configuration.GetSection("FileServer"));

            // Configurar Serilog para logging
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/nfl-fantasy-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Registrar servicios
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IEquipoFantasyService, EquipoFantasyService>();
            builder.Services.AddScoped<IEquipoNFLService, EquipoNFLService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IJugadorService, JugadorService>();
            builder.Services.AddScoped<ILigaService, LigaService>();
            builder.Services.AddScoped<ITemporadaService, TemporadaService>();

            IDbContextProvider contextProvider = new DbContextProvider();
            contextProvider.registerRepositories(builder.Services);

            // Configuración de autenticación JWT
            var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada");
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer no configurado");
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience no configurado");

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
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Configuración mejorada de Swagger
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "NFL Fantasy API",
                    Version = "v1",
                    Description = "API para la gestión de usuarios y equipos de Fantasy NFL",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "Equipo de Desarrollo NFL Fantasy",
                        Email = "support@nflfantasy.com"
                    }
                });

                // Configurar Swagger para usar JWT
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

                // Incluir comentarios XML
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            // Conexión a la base de datos
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada");
            }


            contextProvider.ConfigureDatabase(builder.Services, connectionString);


            // Configuración de CORS
            var myAllowSpecificOrigins = "_myAllowSpecificOrigins";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: myAllowSpecificOrigins,
                    policy =>
                    {
                        policy.WithOrigins(
                                builder.Configuration["Cors:AllowedOrigins"]?.Split(',')
                                ?? new[] { "http://localhost:4200" }
                            )
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "NFL Fantasy API v1");
                    options.RoutePrefix = string.Empty;
                });
            }

            // Middleware de manejo global de excepciones
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";

                    var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                    if (error != null)
                    {
                        var exception = error.Error;
                        Log.Error(exception, "Error no manejado en la aplicación");

                        await context.Response.WriteAsJsonAsync(new
                        {
                            mensaje = "Error interno del servidor",
                            detalles = app.Environment.IsDevelopment() ? exception.Message : null
                        });
                    }
                });
            });

            // Crear estructura de carpetas necesarias
            var uploadsPath = Path.Combine(app.Environment.WebRootPath, "uploads", "equipos");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
                Log.Information("Carpeta de uploads creada: {UploadsPath}", uploadsPath);
            }

            app.UseStaticFiles();

            // Solo usar redirección HTTPS en producción
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseCors(myAllowSpecificOrigins);

            // Agregar middleware de autenticación y autorización
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            Log.Information("Iniciando aplicación NFL Fantasy API con autenticación JWT");

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
        }
    }
}


