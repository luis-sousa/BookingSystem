using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Reflection;
using System.Text;
using UserService.Data;
using UserService.DTOs;
using UserService.Mapping;
using UserService.Models;
using UserService.Repositories;
using UserService.Services;
using UserService.Validators;

var builder = WebApplication.CreateBuilder(args);

// Obter configuração JWT do appsettings.json
var jwtSettings = builder.Configuration.GetSection("JWT");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

// Adicionar autenticação JWT
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
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

// Add DbContext
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add repositories & services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserAuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddAutoMapper(typeof(UserProfile));
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PatchUserValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();

// Add controllers & swagger
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation(); //fui eu que pus por causa do fluent validation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddNewtonsoftJson();  // habilita suporte a JsonPatchDocument

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(2, 0); // versão padrão
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});


// Swagger
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // ex: v1, v2
    options.SubstituteApiVersionInUrl = true;
});


builder.Services.AddSwaggerGen(c =>
{
   //Define manualmente docs para cada versão
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Service API v1", Version = "v1" });
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "User Service API v2", Version = "v2" });

    // Inclui comentários XML no Swagger (crie em Properties -> Build -> XML Documentation)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);

    //habilitar o button authorize no swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT desta forma: Bearer {seu_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Identity PasswordHasher
//builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Aqui você registra seus serviços/repositórios
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

//if (app.Environment.IsProduction())
//{
//    using (var scope = app.Services.CreateScope())
//    {
//        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
//        context.Database.Migrate(); // aplica as migrations e já faz database update se tiver migracoes novas
//    }
//}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    // Configura o SwaggerUI com múltiplas versões
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwaggerUI(c =>
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                $"User Service API {description.GroupName}");
        }
    });

    //using (var scope = app.Services.CreateScope())
    //{
    //    var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    //    if (!context.Users.Any(u => u.Role == "Admin"))
    //    {
    //        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

    //        var admin = new User
    //        {
    //            Username = "luis_admin",
    //            Email = "luis_admin@example.com",
    //            Role = "Admin",
    //            CreatedAt = DateTime.UtcNow,
    //            UpdatedAt = DateTime.UtcNow,
    //            PasswordHash = adminPasswordHash
    //        };

    //        context.Users.Add(admin);
    //        context.SaveChanges();
    //    }
    //}
}

app.UseAuthentication();  // <- ESSENCIAL
app.UseAuthorization();   // <- ESSENCIAL

app.UseHttpsRedirection();
app.MapControllers();

app.Run();