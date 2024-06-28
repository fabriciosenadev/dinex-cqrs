namespace Dinex.Api;

public static class ApiConfig
{
    public const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

    public static IServiceCollection AddApiConfig(this IServiceCollection services, IConfiguration configuration)
    {
        // Add services to the container.
        services.AddControllers();

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        //connection string
        //builder.Services.AddDbContext<DinexBackendContext>(
        //    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DinExDB"))
        //    );

        var appSettings = new AppSettings();
        new ConfigureFromConfigurationOptions<AppSettings>(configuration.GetSection("AppSettings")).Configure(appSettings);
        services.AddSingleton(appSettings);

        // fix to work with this section on classes and methods
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

        #region Controle de versão
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
        #endregion

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        //configuration add to working
        services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                builder =>
                {
                    builder.WithOrigins(appSettings.AllowedHost)
                        .WithMethods("POST", "PUT", "GET", "DELETE", "OPTIONS")
                        .WithHeaders("accept", "content-type", "origin", "authorization");
                });
        });

        // configuration to work with ExcelDataReader --- see readme https://github.com/ExcelDataReader/ExcelDataReader
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        return services;
    }

    public static IApplicationBuilder UseApiConfig(this IApplicationBuilder app, IWebHostEnvironment environment, IServiceProvider serviceScope)
    {
        // Execute Migrations on start app
        using (var scope = serviceScope.CreateScope())
        {
            var dataContext = scope.ServiceProvider.GetRequiredService<DinexApiContext>();
            dataContext.Database.Migrate();
        }

        app.UseRouting();

        if (environment.IsDevelopment())
        {
            app.UseCors(MyAllowSpecificOrigins);
        }
        else
        {
            app.UseCors(MyAllowSpecificOrigins);
            app.UseHsts();
        }

        //app.UseMiddleware<JwtMiddleware>();
        //app.UseMiddleware<ErrorHandlerMiddleware>();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseStaticFiles();

        app.UseEndpoints(options =>
        {
            options.MapControllers();
        });

        return app;
    }
}
