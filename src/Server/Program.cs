using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using NLog;
using NLog.Web;
using PeterPedia.Server.Jobs;
using PeterPedia.Server.Services;
using Quartz;

Logger logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllersWithViews().AddJsonOptions(options => options.JsonSerializerOptions.AddContext<PeterPediaJSONContext>());
    builder.Services.AddRazorPages();

    builder.Services.AddHttpClient();
    builder.Services.AddMemoryCache();

    builder.Services.AddDbContext<PeterPediaContext>(options =>
    {
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        options.UseSqlite(builder.Configuration.GetConnectionString("PeterPedia"));
    });

    builder.Services.AddSingleton(x =>
        new TheMovieDatabaseService(
            builder.Configuration["TheMovieDbAccessToken"],
            x.GetRequiredService<IHttpClientFactory>(),
            x.GetRequiredService<IMemoryCache>()
        ));

    builder.Services.AddScoped<IFileService, FileService>();

    builder.Services.AddQuartz(q =>
    {
        q.UseMicrosoftDependencyInjectionJobFactory();

        q.AddJobAndTrigger<RemoveArticleJob>(builder.Configuration);
        q.AddJobAndTrigger<RefreshArticleJob>(builder.Configuration);
        q.AddJobAndTrigger<VideoJob>(builder.Configuration);
        q.AddJobAndTrigger<ShowUpdateJob>(builder.Configuration);
        q.AddJobAndTrigger<PhotoJob>(builder.Configuration);
        q.AddJobAndTrigger<MovieUpdateJob>(builder.Configuration);
    });

    builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    WebApplication app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseWebAssemblyDebugging();
    }
    else
    {
        app.UseExceptionHandler("/Error");
    }

    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(builder.Configuration["VideoPath"] ?? "/video"),
        RequestPath = "/video"
    });

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(builder.Configuration["PhotoPath"] ?? "/photos"),
        RequestPath = "/photo"
    });

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(builder.Configuration["ImagePath"] ?? "/images"),
        RequestPath = "/images"
    });

    app.UseRouting();

    app.UseEndpoints(app =>
    {
        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");
    });

    CreateDbIfNotExists(logger, app);

    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    LogManager.Shutdown();
}

static void CreateDbIfNotExists(Logger logger, WebApplication app)
{
    try
    {
        PeterPediaContext? lpdaContext = app.Services.GetRequiredService<PeterPediaContext>();
        DbInitializer.Initialize(lpdaContext);
    }
    catch (Exception ex)
    {
        logger.Error(ex, "An error occurred creating the DB.");

        throw;
    }
}
