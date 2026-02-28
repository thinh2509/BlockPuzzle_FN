using BLL.Services;
using DAL;
using DAL.Repositories;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebAPI.Configuration;
using Microsoft.OpenApi.Models;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    // Configure MongoDB settings
    builder.Services.Configure<MongoDBSettings>(
        builder.Configuration.GetSection("MongoDBSettings"));

    // Configure JWT settings
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
    builder.Services.AddSingleton(jwtSettings); // Register JwtSettings as a singleton

    // Configure JWT Authentication
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
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

    // Register repositories and services
    builder.Services.AddSingleton<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUserService, UserService>();


    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(option =>
    {
        option.SwaggerDoc("v1", new OpenApiInfo { Title = "GameAPI", Version = "v1" });
        option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        option.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type=ReferenceType.SecurityScheme,
                        Id="Bearer"
                    }
                },
                new string[]{}
            }
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (ReflectionTypeLoadException ex)
{
    Console.WriteLine("--- DETAILED REFLECTION TYPE LOAD EXCEPTION ---");
    Console.WriteLine(ex.Message);
    Console.WriteLine("--- LOADER EXCEPTIONS ---");
    if (ex.LoaderExceptions != null && ex.LoaderExceptions.Length > 0)
    {
        foreach (var loaderEx in ex.LoaderExceptions)
        {
            if (loaderEx != null)
            {
                 Console.WriteLine(loaderEx.Message);
            }
        }
    }
    else
    {
        Console.WriteLine("No loader exceptions were found.");
    }
    Console.WriteLine("--- END OF EXCEPTION DETAILS ---");
    throw; // Re-throw the exception after logging.
}