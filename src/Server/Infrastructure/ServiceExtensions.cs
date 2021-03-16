using System;
using System.Linq;
using System.Text;
using MicroBlog.Server.API.Infrastructure.Contexts;
using MicroBlog.Server.API.Infrastructure.Seeds;
using MicroBlog.Server.Models.Identity;
using MicroBlog.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using Serilog;

namespace MicroBlog.Server.API.Infrastructure
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// add database services, swagger service configuration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="environment"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddInfrastructures(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
        {
            services.AddCustomCors();
            services.AddCustomSwagger();
            services.AddCustomIdentity(configuration);

            var blogConfigurations = configuration.GetSection("Blog");

            string connectionString = blogConfigurations.GetValue<string>("ConnectionString");
            bool useInMemory = blogConfigurations.GetValue<bool>("UseInMemory");
            bool useSqlite = blogConfigurations.GetValue<bool>("UseSqLite");

            services.AddDbContext<BlogDbContext>(options =>
            {
                if (useSqlite)
                {
                    options.UseSqlite(connectionString, b =>
                    {
                        b.MigrationsAssembly(typeof(BlogDbContext).Assembly.FullName);
                    });
                    // ignore sqlite transaction warning
                    options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
                }
                else
                {
                    options.UseInMemoryDatabase("MicroBlog"); // Takes database name
                }

                if (environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
            });


            services.AddScoped<IBlogDbContext>(provider =>
            provider.GetService<BlogDbContext>());
            services.AddScoped<IDbInitializerService, DbInitializerService>();

            return services;
        }

        private static IServiceCollection AddCustomCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("DefaultCorsPolicy",
                    builder =>
                    {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    });
            });

            return services;
        }

        private static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            #region Swagger & OpenApi Configuration

            services.AddOpenApiDocument(configure =>
            {
                configure.Title = "Micro Blog API";
                configure.AddSecurity("JWT", Enumerable.Empty<string>(),
                    new NSwag.OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Description = "Type into the textbox: Bearer {your JWT token}."
                    });

                configure.OperationProcessors.Add(
                    new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });

            // Customise default API behaviour
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "BlazorMicroBlog.Api",
                    Version = "v1"
                });
            });
            #endregion

            return services;
        }

        /// <summary>
        /// add identity and configure user security options. Configure Token Authentication and Validation
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private static IServiceCollection AddCustomIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection IdentityOptions = configuration.GetSection("IdentityOptions");
            IConfigurationSection jwtSettings = configuration.GetSection("JwtSettings");

            services.AddIdentity<UserInfo, IdentityRole>((opt) =>
            {
                // unique Email address per account
                opt.User.RequireUniqueEmail = IdentityOptions.GetValue<bool>("RequireUniqueEmail");
                // account security and lock setings
                if (IdentityOptions.GetValue<bool>("LockoutEnabled"))
                {
                    opt.Lockout.AllowedForNewUsers = true;
                    opt.Lockout.MaxFailedAccessAttempts = IdentityOptions.GetValue<int>("MaxFailedAccessAttempts");
                    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(IdentityOptions.GetValue<double>("LockoutTimeSpan"));
                }
                // password complexity and conditions
                if (IdentityOptions.GetValue<bool>("HighPasswordComplexity") is false)
                {
                    opt.Password.RequiredUniqueChars = 0;
                    opt.Password.RequireNonAlphanumeric = false;
                    opt.Password.RequireUppercase = false;
                }
            }).AddEntityFrameworkStores<BlogDbContext>();

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    RequireSignedTokens = true,
                    ValidIssuer = jwtSettings.GetValue<string>("validIssuer"),
                    ValidAudience = jwtSettings.GetValue<string>("validAudience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.GetValue<string>("securityKey")))
                };
            });
            services.AddScoped<ITokenService, TokenService>();
            return services;
        }
    }
}
