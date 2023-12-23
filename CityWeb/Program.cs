using City_Web.Models.Data;
using CityWeb.Auth;
using NLog;
using CityWeb.Helper;
using CityWeb.Model.Interface;
using CityWeb.Model.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Extensions.Logging;
using NLog.Web;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using WebApplication10.Models.Interface;
using WebApplication10.Models.Repository;
using NLog.Fluent;
using StackExchange.Redis;

//NLog.LogManager.LogFactory.SetCandidateConfigFilePaths(new List<string> { $"{Path.Combine(pathToContentRoot, "nlog.config")}" });

var logger = LogManager.Setup().LoadConfigurationFromAppSettings()
            .GetCurrentClassLogger();
logger.Debug("init main");
try
{
    var builder = WebApplication.CreateBuilder(args);
    ConfigurationManager configuration = builder.Configuration;
    builder.Services.AddSingleton<IConnectionMultiplexer>(opt =>
        ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("10.51.10.79,1446")));
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    });

    //For Identity
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();


    //Add Authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

    })
    // Adding Jwt Brearer
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidAudience = configuration["JWT:ValidAudience"],
            ValidIssuer = configuration["JWT:ValidIssuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]))
        };
    });


    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddScoped<ICityRepository, CityRepository>();
    builder.Services.AddScoped<ICountryRepository, CountryRepository>();
    builder.Services.AddScoped<IWeatherRepository, WRepository>();
    builder.Services.AddScoped<IJwtManagerRepository, JwtManagerRepository>();
    builder.Services.AddScoped<ICacheService, CacheSrvice>();
    builder.Services.AddScoped<IWeatherService, WeatherService>();


    builder.Services.AddAutoMapper(typeof(MappingProfiles));

    builder.Services.AddHttpClient<WeatherService>();
    builder.Services.AddSingleton<WeatherService>();
    builder.Services.AddControllers();

    builder.Services.AddSwaggerGen(
        opt =>
        {
            opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
            opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Enter token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });
            opt.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI( c=> c.SwaggerEndpoint("/swagger/v1/swagger.json", "RedisCacheDemo v1"));
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex);
}
finally
{
    LogManager.Shutdown();
}
