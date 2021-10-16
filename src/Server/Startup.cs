using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using Microsoft.OpenApi.Models;
using PeterPedia.Server.Data;
using Microsoft.EntityFrameworkCore;
using PeterPedia.Server.Services;
using System.Net.Http;

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

            services.AddScoped<RemoveArticleService>();
            services.AddScoped<RefreshArticleService>();
            services.AddScoped<IShowUpdateService, ShowUpdateService>();
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

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
