using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Gateway.Application.Extensions;
using Gateway.Application.Middlewares;

namespace Gateway.Application;

public class Startup
{
    private IConfiguration Configuration { get; }
    
    private IWebHostEnvironment Environment { get; }

    public Startup(IWebHostEnvironment environment)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(environment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

        Environment = environment;
        Configuration = builder.Build();
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        #region --- Hangfire ---
        if (Environment.IsDevelopment())
        {
            services.AddHangfire(configuration =>
                configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseDefaultTypeSerializer()
                    .UseMemoryStorage()
                );
        }
        else
        {
            services.AddHangfire(configuration => configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(
                        Configuration.GetConnectionString("Hangfire"),
                        new SqlServerStorageOptions
                        {
                            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                            QueuePollInterval = TimeSpan.Zero,
                            UseRecommendedIsolationLevel = true,
                            DisableGlobalLocks = true
                        }
                    ));
        }
        services.AddHangfireServer();
        #endregion

        #region --- Versioning ---
        services.AddApiVersioning(_ =>
        {
            _.DefaultApiVersion = new ApiVersion(1, 0);
            _.AssumeDefaultVersionWhenUnspecified = true;
            _.ReportApiVersions = true;
        });

        services.AddVersionedApiExplorer(_ =>
        {
            _.GroupNameFormat = "'v'VVV";
            _.SubstituteApiVersionInUrl = true;
        });
        #endregion
        
        /* For request body size */
        services.Configure<IISServerOptions>(options =>  options.MaxRequestBodySize = int.MaxValue);
        services.Configure<KestrelServerOptions>(options => options.Limits.MaxRequestBodySize = int.MaxValue);
        /* For controller */
        services.AddControllers();
        /* For Policy */
        services.AddCorsPolicy();
        /* For JWT */
        services.AddJwt(Configuration["Jwt:Key"], Configuration["Jwt:Issuer"], Configuration["Jwt:Issuer"]);
        /* For Swagger */
        services.AddSwagger(Environment, Configuration);
        /* For mapping */
        services.AddMappingProfile();
        /* For Layer Pattern */
        services.AddRepositories();
        services.AddBusinessServices();
        /* For Context */
    }
    
    public void Configure(IApplicationBuilder application, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
            application.UseDeveloperExceptionPage();
        /* Hangfire */
        application.UseHangfireDashboard("/hangfire", new DashboardOptions()
        {
            Authorization = new List<IDashboardAuthorizationFilter>()
        });
        /* Policy */
        application.UseCors("AllowAll");
        /* Middleware */
        application.UseMiddleware<ErrorHandlerMiddleware>();
        /* For route */
        application.UseHttpsRedirection();
        application.UseRouting();
        /* For authentication */
        application.UseAuthentication();
        application.UseAuthorization();
        /* For static file */
        application.UseStaticFiles(
            new StaticFileOptions
            {
                OnPrepareResponse = _ =>
                {
                    if (_.Context.Request.Path.StartsWithSegments("/files"))
                    {
                        _.Context.Response.Headers.Add("Cache-Control", "no-store");
                        if (_.Context?.User?.Identity != null && !_.Context.User.Identity.IsAuthenticated)
                        {
                            _.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            _.Context.Response.ContentLength = 0;
                            _.Context.Response.Body = Stream.Null;
                            _.Context.Response.Redirect("/");
                        }
                    }
                }
            }
        );
        /* For endpoint */
        application.UseEndpoints(_ =>  _.MapControllers());
        /* For swagger */
        application.UseOpenApi();
        application.UseSwaggerUi3(_ => _.CustomStylesheetPath = "/styles/swagger.css");
        /* For hangfire */
    }
}
