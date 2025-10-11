using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

//SHARED
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Frock_backend.shared.Infrastructure.Interfaces.ASP.Configuration;
using Frock_backend.shared.Domain.Repositories;

//IAM - Adrian
using Frock_backend.IAM.Application.Internal.CommandServices;
using Frock_backend.IAM.Application.Internal.OutboundServices;
using Frock_backend.IAM.Application.Internal.QueryServices;

using Frock_backend.IAM.Domain.Repositories;
using Frock_backend.IAM.Domain.Services;
using Frock_backend.IAM.Infrastructure.Persistence.EFC.Repositories;

using Frock_backend.IAM.Infrastructure.Hashing.BCrypt.Services;
using Frock_backend.IAM.Infrastructure.Pipeline.Middleware.Extensions;
using Frock_backend.IAM.Infrastructure.Tokens.JWT.Configuration;
using Frock_backend.IAM.Infrastructure.Tokens.JWT.Services;

using Frock_backend.IAM.Interfaces.ACL;
using Frock_backend.IAM.Interfaces.ACL.Services;

//COMPANY - Yasser

using Frock_backend.transport_Company.Application.Internal.CommandServices;
using Frock_backend.transport_Company.Application.Internal.QueryServices;

using Frock_backend.transport_Company.Domain.Repositories;
using Frock_backend.transport_Company.Domain.Services;

using Frock_backend.transport_Company.Infrastructure.Repositories;

//STOPS - Amir

using Frock_backend.stops.Application.Internal.CommandServices;
using Frock_backend.stops.Application.Internal.QueryServices;

using Frock_backend.stops.Domain.Repositories;
using Frock_backend.stops.Domain.Services;

using Frock_backend.stops.Infrastructure.Repositories;

//GEOGRAPHIC - Amir
using Frock_backend.stops.Application.Internal.CommandServices.Geographic;
using Frock_backend.stops.Application.Internal.QueryServices.Geographic;

using Frock_backend.stops.Domain.Repositories.Geographic;
using Frock_backend.stops.Domain.Services.Geographic;

using Frock_backend.stops.Infrastructure.Repositories.Geographic;

using Frock_backend.stops.Infrastructure.Seeding;
// ROUTES - GSUS
using Frock_backend.routes.Domain.Repository;
using Frock_backend.routes.Infrastructure.Repositories;
using Frock_backend.routes.Domain.Service;
using Frock_backend.routes.Application.Internal.CommandServices;
using Frock_backend.routes.Application.Internal.QueryServices;
using Frock_backend.shared.Domain.Services;
using Frock_backend.shared.Infrastructure.Configuration;
using Frock_backend.shared.Infrastructure.Services;
using Frock_backend.stops.Application.External;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure Lower Case URLs
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Configure Kebab Case Route Naming Convention
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
     options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "Frock_Backend",
            Version = "v1",
            Description = "Frock Backend API",
            TermsOfService = new Uri("https://acme-learning.com/tos"),
            Contact = new OpenApiContact
            {
                Name = "frock Studios",
                Email = "frockWEB.com"
            },
            License = new OpenApiLicense
            {
                Name = "Apache 2.0",
                Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0.html")
            }
        });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

/// <summary>
/// Obtiene la cadena de conexión a la base de datos MySQL desde la configuración de la aplicación.
/// </summary>
/// <remarks>
/// El valor se extrae de la sección "ConnectionStrings" del archivo `appsettings.json`,
/// buscando la clave "DefaultConnection".
/// </remarks>
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (connectionString is null)
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

// Configure Database Context and Logging Levels
if (builder.Environment.IsDevelopment())
    builder.Services.AddDbContext<AppDbContext>(
        options =>
        {
            options.UseMySQL(connectionString)
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        });
else if (builder.Environment.IsProduction())
    builder.Services.AddDbContext<AppDbContext>(
        options =>
        {
            options.UseMySQL(connectionString)
                .LogTo(Console.WriteLine, LogLevel.Error)
                .EnableDetailedErrors();
        });


// Configure Dependency Injection

// Shared Bounded Context Injection Configuration
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// IAM Bounded Context Injection Configuration
// TokenSettings Configuration
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddScoped<IIamContextFacade, IamContextFacade>();

//Company
    builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
    builder.Services.AddScoped<ICompanyCommandService, CompanyCommandService>();
    builder.Services.AddScoped<ICompanyQueryService, CompanyQueryService>();

//Geographic
builder.Services.AddScoped<IRegionRepository, RegionRepository>();
    builder.Services.AddScoped<IRegionCommandService, RegionCommandService>();
    builder.Services.AddScoped<IRegionQueryService, RegionQueryService>();
        /**/
    builder.Services.AddScoped<IProvinceRepository, ProvinceRepository>();
    builder.Services.AddScoped<IProvinceCommandService, ProvinceCommandService>();
    builder.Services.AddScoped<IProvinceQueryService, ProvinceQueryService>();
        /**/
    builder.Services.AddScoped<IDistrictRepository, DistrictRepository>();
    builder.Services.AddScoped<IDistrictCommandService, DistrictCommandService>();
    builder.Services.AddScoped<IDistrictQueryService, DistrictQueryService>();
        /**/

//Stops
    builder.Services.AddScoped<IStopRepository, StopRepository>();
    builder.Services.AddScoped<IStopCommandService, StopCommandService>();
    builder.Services.AddScoped<IStopQueryService, StopQueryService>();

//Routes
    builder.Services.AddScoped<IRouteRepository, RouteRepository>();
    builder.Services.AddScoped<IRouteCommandService, RouteCommandService>();
builder.Services.AddScoped<IRouteQueryService, RouteQueryService>();

//GEOSERVICE
    builder.Services.AddHttpClient<IGeoImportService, GeoImportService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["GeoApi:BaseUrl"]);
    });
//Seeding Service Geographic Data
// Datos iniciales fijos de datos geográficos
builder.Services.AddScoped<GeographicDataSeeder>();


//CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://deft-tapioca-c27a9c.netlify.app", "https://frock-front-end.vercel.app")//ajustar
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Cloudinary Configuration
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

var app = builder.Build();

app.UseCors();

// Verify Database Objects are created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

  // Seed initial geographic data
    try
    {
        var seeder = services.GetRequiredService<GeographicDataSeeder>();
        await seeder.SeedDataAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error durante la carga de datos iniciales.");
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger(c =>
{
    c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    c.RoutePrefix = string.Empty; // Opcional: para que Swagger sea la p�gina ra�z
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});

app.UseHttpsRedirection();
app.UseRouting(); // Si no está implícito
app.UseRequestAuthorization(); // Tu middleware personalizado
app.UseAuthorization(); // Authorization de ASP.NET Core
app.MapControllers();

app.Run();
