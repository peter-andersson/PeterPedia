using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using PeterPedia.Jobs;
using Quartz;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console()
        .ReadFrom.Configuration(ctx.Configuration));

    Log.Information("Starting web host");

    // Add services to the container.
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();

    builder.Services.AddHttpClient();
    builder.Services.AddMemoryCache();

    builder.Services.AddDbContext<PeterPediaContext>(options =>
    {
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        options.UseSqlite(builder.Configuration.GetConnectionString("PeterPedia"));
    });

    builder.Services.AddScoped(x =>
        new TheMovieDatabaseService(
            builder.Configuration["TheMovieDbAccessToken"],
            x.GetRequiredService<IHttpClientFactory>(),
            x.GetRequiredService<IMemoryCache>()
        ));

    builder.Services.AddScoped<Navigation>();

    builder.Services.AddScoped<IAuthorManager, AuthorManager>();
    builder.Services.AddScoped<IBookManager, BookManager>();
    builder.Services.AddScoped<IEpisodeManager, EpisodeManager>();
    builder.Services.AddScoped<IFileService, FileService>();
    builder.Services.AddScoped<IMovieManager, MovieManager>();
    builder.Services.AddScoped<IReaderManager, ReaderManager>();

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

    WebApplication app = builder.Build();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
    }

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

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(builder.Configuration["LogsPath"] ?? "/logs"),
        RequestPath = "/logs"
    });

    app.UseRouting();

    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");

    CreateDbIfNotExists(app);

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Stopped program because of exception");
    throw;
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

static void CreateDbIfNotExists(WebApplication app)
{
    IServiceScopeFactory? scopedFactory = app.Services.GetService<IServiceScopeFactory>();

    if (scopedFactory is null)
    {
        Log.Error("ScopedFactory is null");
        return;
    }

    using IServiceScope scope = scopedFactory.CreateScope();
    try
    {
        PeterPediaContext? lpdaContext = scope.ServiceProvider.GetService<PeterPediaContext>();
        if (lpdaContext is not null)
        {
            DbInitializer.Initialize(lpdaContext);
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred creating the DB.");

        throw;
    }
}
