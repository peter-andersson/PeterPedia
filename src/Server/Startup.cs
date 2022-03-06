using Microsoft.EntityFrameworkCore;
using PeterPedia.Server.Services;
using Microsoft.Extensions.FileProviders;

namespace PeterPedia.Server;

public class Startup
{
    public Startup(IConfiguration configuration) => Configuration = configuration;

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {

        services.AddControllersWithViews().AddJsonOptions(options => options.JsonSerializerOptions.AddContext<PeterPediaJSONContext>()); ;
        services.AddRazorPages();

        services.AddHttpClient();

        services.AddDbContext<PeterPediaContext>(options =>
        {
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.UseSqlite(Configuration.GetConnectionString("PeterPedia"));
        });

        services.AddSingleton(x =>
            new TheMovieDatabaseService(
                Configuration["TheMovieDbAccessToken"],
                x.GetRequiredService<IHttpClientFactory>()
            ));

        services.AddScoped<IFileService, FileService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
        }

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Configuration["VideoPath"] ?? "/video"),
            RequestPath = "/video"
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Configuration["PhotoPath"] ?? "/photos"),
            RequestPath = "/photo"
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Configuration["ImagePath"] ?? "/images"),
            RequestPath = "/images"
        });

        app.UseRouting();

        app.UseEndpoints(app =>
        {
            app.MapRazorPages();
            app.MapControllers();
            app.MapFallbackToFile("index.html");
        });        
    }
}
