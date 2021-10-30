using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PeterPedia.Server.Data;
using Microsoft.EntityFrameworkCore;
using PeterPedia.Server.Services;
using System.Net.Http;
using Microsoft.Extensions.FileProviders;

namespace PeterPedia.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddSwaggerGen(c =>
                        {
                            c.SwaggerDoc("v1", new OpenApiInfo { Title = "PeterPedia.Server", Version = "v1" });
                        });

            services.AddHttpClient();

            services.AddDbContext<PeterPediaContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("PeterPedia"));
            });

            services.AddSingleton(x =>
                new TheMovieDatabaseService(
                    Configuration["TheMovieDbAccessToken"],
                    x.GetRequiredService<IHttpClientFactory>()
                ));

            services.AddHostedService<ConsumeRemoveArticleService>();
            services.AddHostedService<ConsumeRefreshArticleService>();
            services.AddHostedService<ConsumeShowUpdateService>();
            services.AddHostedService<ConsumeVideoService>();

            services.AddScoped<VideoService>();
            services.AddScoped<RemoveArticleService>();
            services.AddScoped<RefreshArticleService>();
            services.AddScoped<IShowUpdateService, ShowUpdateService>();
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

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LPDA.Api v1"));

            app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/Book"), book =>
            {
                book.UseBlazorFrameworkFiles("/Book");
                book.UseStaticFiles();

                book.UseRouting();
                book.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("Book/{*path:nonfile}", "Book/index.html");
                });
            });

            app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/ReadList"), bookmark =>
            {
                bookmark.UseBlazorFrameworkFiles("/ReadList");
                bookmark.UseStaticFiles();

                bookmark.UseRouting();
                bookmark.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("ReadList/{*path:nonfile}", "ReadList/index.html");
                });
            });

            app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/Episodes"), episodes =>
            {
                episodes.UseBlazorFrameworkFiles("/Episodes");
                episodes.UseStaticFiles();

                episodes.UseRouting();
                episodes.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("Episodes/{*path:nonfile}", "Episodes/index.html");
                });
            });

            app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/Movie"), movie =>
            {
                movie.UseBlazorFrameworkFiles("/Movie");
                movie.UseStaticFiles();

                movie.UseRouting();
                movie.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("Movie/{*path:nonfile}", "Movie/index.html");
                });
            });

            app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/Reader"), reader =>
            {
                reader.UseBlazorFrameworkFiles("/Reader");
                reader.UseStaticFiles();

                reader.UseRouting();
                reader.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("Reader/{*path:nonfile}", "Reader/index.html");
                });
            });

            app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/VideoPlayer"), reader =>
            {
                reader.UseBlazorFrameworkFiles("/VideoPlayer");
                reader.UseStaticFiles();

                reader.UseRouting();
                reader.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("VideoPlayer/{*path:nonfile}", "VideoPlayer/index.html");
                });
            });

            app.UseStaticFiles();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Configuration["VideoPath"] ?? "/video"),
                RequestPath = "/video"
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
