using System.Linq;
using Application.Server.API.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag;
using NSwag.Generation.Processors.Security;
using Serilog;

namespace Application.Server.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            HostingEnvironment = hostingEnvironment;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews(opt =>
            {
                opt.ReturnHttpNotAcceptable = true;
                opt.CacheProfiles.Add("30SecCache", new CacheProfile
                {
                    Duration = 30
                });
            }).AddJsonOptions((options) =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    //options.JsonSerializerOptions.WriteIndented = true;
                });
            services.AddRazorPages();
            services.AddResponseCaching();
            services.AddInfrastructures(HostingEnvironment, Configuration);

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
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseSerilogRequestLogging(); // req/res logging
            app.UseHttpsRedirection(); // use https

            app.UseCors((policy) =>
            {
                policy.AllowAnyOrigin();
                policy.AllowAnyMethod();
                policy.AllowAnyHeader();
            });

            #region swagger and ui middlewares
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlazorMicroBlog.Api v1"));
            app.UseOpenApi();
            app.UseSwaggerUi3(settings =>
            {
                settings.Path = "/wwwroot/api";
                settings.DocumentPath = "/wwwroot/api/specification.json";
            });
            #endregion

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();


            app.UseResponseCaching();

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
